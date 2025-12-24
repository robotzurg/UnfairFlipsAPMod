using System;
using System.Collections;
using System.Numerics;
using HarmonyLib;
using Tweens;
using Tweens.Core;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace UnfairFlipsAPMod;

public class GameHandler : MonoBehaviour
{
    private static Coroutine _queuedAutoFlip;
    private static CoinFlip _coinFlip;
    
    public void InitOnConnect()
    {
        UpdateCoinValue();
        AutoFlipIconHandler.CreateButton();
        _coinFlip = FindObjectOfType<CoinFlip>();
        _coinFlip.tutorialMessages =
        [
            CreateTutorialMessage("Welcome to Unfair Flips Archipelago!", 2),
            CreateTutorialMessage("This game doesn't have a credits screen", 3),
            CreateTutorialMessage("So we'll list off all the cool people who helped make this a reality here", 4),
            CreateTutorialMessage("Developer - xMcacutt", 5),
            CreateTutorialMessage("Client Development - JeffDev", 6),
            CreateTutorialMessage("apworld Logic - itepastra (Noa)", 7),
            CreateTutorialMessage("apworld Support - DashieSwag92", 8),
            CreateTutorialMessage("Testing & Support - Sterlia, EthicalLogic, Peppidesu, Mac", 9),
            CreateTutorialMessage("May the odds... idk do whatever they feel like", 10),
            CreateTutorialMessage("WOW you did a hundred flips. That's almost a hundred and one. Goo:) Job", 100),
            CreateTutorialMessage("WOW you did a hundred and one flips. That's almost a hundred and two. Goo:) Job!", 101),
            CreateTutorialMessage("Okay I'll stop now...", 102),
            CreateTutorialMessage("Eight Eight Eight Eight Eight Eight Eight Eight", 888),
            CreateTutorialMessage("Nice", 69),
            CreateTutorialMessage("You are statistically likely to finish this game.", 300),
            CreateTutorialMessage("The new button in the top right turns on autoflip... In case you're feeling too lazy to press a button.", 200),
            CreateTutorialMessage("You realise your friends want to finish this generation right?", 1000)
        ];
        QueueNextAutoFlip();
    }

    private CoinFlip.TutorialMessage CreateTutorialMessage(string message, int flipNum)
    {
        var tutorialMessage = new CoinFlip.TutorialMessage();
        tutorialMessage.flipnum = flipNum;
        tutorialMessage.message = message;
        return tutorialMessage;
    }

    public void UpdateCoinValue()
    {
        var num = 1;
        var color = Color.white;
        var vector3 = Vector3.one;
        var coinType = 0;
        switch (UnfairFlipsAPMod.SaveDataHandler.SaveData.CoinUpgradeLevel)
        {
            case 1:
                num = 5;
                color = new Color(0.9f, 0.9f, 0.9f, 1f);
                coinType = 1;
                break;
            case 2:
                num = 10;
                color = new Color(1f, 1f, 1f, 1f);
                vector3 = Vector3.one * 0.7f;
                coinType = 1;
                break;
            case 3:
                num = 25;
                vector3 = Vector3.one * 1.2f;
                coinType = 1;
                break;
            case >= 4:
                num = 100;
                vector3 = Vector3.one * 1.2f;
                coinType = 2;
                break;
        }
        UnfairFlipsAPMod.SaveDataHandler.SaveData.CoinValue = num;
        FindObjectOfType<CoinFlip>().GetComponent<Image>().color = color;
        FindObjectOfType<CoinFlip>().transform.localScale = vector3;
        FindObjectOfType<CoinFlip>().SetCoinType(coinType);
    }

    private static BigInteger _flipMoneyResult;
    [HarmonyPatch(typeof(FlipResultMessage))]
    private class FlipResultMessage_Patch
    {
        [HarmonyPatch("ShowResult")]
        [HarmonyPrefix]
        public static bool ShowResult(FlipResultMessage __instance, bool heads, long money, int comboNum)
        {
            __instance.gameObject.CancelTweens();
            __instance.text.text = !heads ? "TAILS" : $"HEADS {comboNum}X\n{(_flipMoneyResult > 0L ? Mathy.CentsToDollarString(_flipMoneyResult) : "")}";
            _flipMoneyResult = 0;
            __instance.text.color = Color.white;
            var anchoredPositionYtween = new AnchoredPositionYTween();
            anchoredPositionYtween.from = __instance.baseYPos;
            anchoredPositionYtween.to = __instance.baseYPos + 150f;
            anchoredPositionYtween.duration = 1f;
            anchoredPositionYtween.easeType = EaseType.QuartOut;
            __instance.gameObject.AddTween(anchoredPositionYtween);
            var colorTween = new Tweens.ColorTween();
            colorTween.from = Color.white;
            colorTween.to = new Color(1f, 1f, 1f, 0.0f);
            colorTween.duration = 1f;
            colorTween.easeType = EaseType.Linear;
            colorTween.onUpdate = (_, value) => __instance.text.color = value;
            __instance.gameObject.AddTween(colorTween);
            return false;
        }
    }
    
    
    [HarmonyPatch(typeof(CoinFlip))]
    private class CoinFlip_Patch
    {
        private static SaveDataHandler SaveManager => UnfairFlipsAPMod.SaveDataHandler;
        private static SlotData SlotData => UnfairFlipsAPMod.SlotData;
        
        [HarmonyPatch("DoFlip")]
        [HarmonyPrefix]
        private static bool DoFlip_Prefix(CoinFlip __instance)
        {
            if (_queuedAutoFlip != null)
            {
                __instance.StopCoroutine(_queuedAutoFlip);
                _queuedAutoFlip = null;
            }
            if (__instance.isFlipping)
                return false;
            __instance.StartCoroutine(Flip(__instance));
            return false;
        }

        private static void InitFlip(CoinFlip coinFlip)
        {
            coinFlip.flippedThisSession = true;
            ++coinFlip.numFlips;
            coinFlip.img.sprite = coinFlip.coinColors[coinFlip.currentCoin].coinSmirk;
            coinFlip.isFlipping = true;
            coinFlip.currentFlipDuration = 0.0f;
            coinFlip.currentFlipAnimateDuration = coinFlip.flipAnimateDuration;
            coinFlip.audioSource.volume = 1f;
        }

        private static IEnumerator Flip(CoinFlip coinFlip)
        {
            InitFlip(coinFlip);

            for (var index = 0; index < coinFlip.tutorialMessages.Length; ++index)
            {
                if (coinFlip.tutorialMessages[index].flipnum == coinFlip.numFlips)
                    coinFlip.messageManager.ShowMessage($"<color=#bbffbb>{coinFlip.tutorialMessages[index].message}</color>");
            }
            
            var flipTime = SaveManager.SaveData.FlipTime;
            if (SaveManager.SaveData.QueuedSlowTraps > 0)
            {
                SaveManager.SaveData.QueuedSlowTraps--;
                flipTime = 10f;
            }
            
            var anchoredPositionTween1 = new AnchoredPositionTween();
            anchoredPositionTween1.from = coinFlip.GetComponent<RectTransform>().anchoredPosition;
            anchoredPositionTween1.to = coinFlip.flipTarget;
            anchoredPositionTween1.duration = flipTime / 2f;
            anchoredPositionTween1.delay = 0.0f;
            anchoredPositionTween1.easeType = (EaseType) 21;
            coinFlip.gameObject.AddTween(anchoredPositionTween1);
            
            var anchoredPositionTween2 = new AnchoredPositionTween();
            anchoredPositionTween2.from = coinFlip.flipTarget;
            anchoredPositionTween2.to = coinFlip.flipBasePosition;
            anchoredPositionTween2.duration = flipTime / 2f;
            anchoredPositionTween2.delay = flipTime / 2f;
            anchoredPositionTween2.easeType = (EaseType) 30;
            coinFlip.gameObject.AddTween(anchoredPositionTween2);
          
            var flipTurnMultiplier = (float)Random.Range(3, 6);
            
            var maxHeadsAllowed = 1 + (SaveManager.SaveData.Fairness * 2);
            var canBeHeads = (coinFlip.headsComboNum + 1) <= maxHeadsAllowed;
            canBeHeads = canBeHeads && SaveManager.SaveData.QueuedTailsTraps == 0;
            if (SaveManager.SaveData.QueuedTailsTraps > 0)
                SaveManager.SaveData.QueuedTailsTraps--;
            var heads = canBeHeads && Random.Range(0.0f, 1f) < (double) SaveManager.SaveData.HeadsChance;
            
            if (heads != coinFlip.prevWasHeads)
                flipTurnMultiplier += 0.5f;
            var comboFailed = false;
            coinFlip.audioSource.clip = coinFlip.headsComboNum != SlotData.RequiredHeads - 1 ? coinFlip.flipSounds[Random.Range(0, coinFlip.flipSounds.Length)] : coinFlip.finalFlipSound;
            coinFlip.audioSource.Play();
          
            if (heads)
                ++coinFlip.headsComboNum;
            else
            {
                if (coinFlip.headsComboNum > 2)
                    comboFailed = true;
                if (coinFlip.headsComboNum >= SlotData.DeathLinkMinStreak && SlotData.DeathLink)
                {
                    var sendDeath = Random.Range(0.0f, 1f) < (double)SlotData.DeathLinkChance / 100;
                    if (sendDeath)
                        UnfairFlipsAPMod.ArchipelagoHandler.SendDeath();
                }
                coinFlip.headsComboNum = 0;
            }
            
            if (heads && coinFlip.headsComboNum == SlotData.RequiredHeads)
            {
                coinFlip.panelManager.SetPanelArrangement(4);
                coinFlip.flipButton.enabled = false;
                UnfairFlipsAPMod.ArchipelagoHandler.Release();
                coinFlip.gameObject.CancelTweens(false);
                var anchoredPositionTween3 = new AnchoredPositionTween();
                anchoredPositionTween3.from = coinFlip.flipBasePosition;
                anchoredPositionTween3.to = coinFlip.flipTarget;
                anchoredPositionTween3.duration = 3f;
                anchoredPositionTween3.delay = 0.0f;
                anchoredPositionTween3.easeType = (EaseType) 21;
                var anchoredPositionTween4 = new AnchoredPositionTween();
                anchoredPositionTween4.from = coinFlip.flipTarget;
                anchoredPositionTween4.to = coinFlip.flipBasePosition;
                anchoredPositionTween4.duration = 3f;
                anchoredPositionTween4.delay = 3f;
                anchoredPositionTween4.easeType = (EaseType)30;
                coinFlip.gameObject.AddTween(anchoredPositionTween3);
                coinFlip.gameObject.AddTween(anchoredPositionTween4);
                var coinSprite = 0;
                const int totalFlips = 3;
                var numFancyFlips = 1;
                while (coinFlip.currentFlipDuration < 6.0)
                {
                    coinFlip.img.sprite = coinFlip.coinColors[coinFlip.currentCoin].coinSlowFlip[coinSprite];
                    ++coinSprite;
                    if (coinSprite >= coinFlip.coinColors[coinFlip.currentCoin].coinSlowFlip.Length)
                    {
                        if (numFancyFlips < totalFlips)
                        {
                            coinSprite = 0;
                            ++numFancyFlips;
                        }
                        else
                            coinSprite = coinFlip.coinColors[coinFlip.currentCoin].coinSlowFlip.Length - 1;
                    }
                    var seconds = 6f / (totalFlips * coinFlip.coinColors[coinFlip.currentCoin].coinSlowFlip.Length);
                    coinFlip.currentFlipDuration += seconds;
                    yield return new WaitForSeconds(seconds);
                }
                coinFlip.audioSource.clip = coinFlip.headsSounds[0];
                coinFlip.audioSource.Play();
                _flipMoneyResult = 0;
                FindObjectOfType<FlipResultMessage>().ShowResult(true, 0L, SlotData.RequiredHeads);
                yield return new WaitForSeconds(1f);
                var objectsOfType = Object.FindObjectsOfType<RocketSpawner>();
                objectsOfType[0].SpawnRockets(0.0f);
                objectsOfType[1].SpawnRockets(1f);
                yield return new WaitForSeconds(4f);
                var localScaleTween = new LocalScaleTween();
                localScaleTween.from = Vector3.zero;
                localScaleTween.to = Vector3.one;
                localScaleTween.duration = 5f;
                localScaleTween.easeType = 0;
                coinFlip.gooJobSticker.AddTween(localScaleTween);
                coinFlip.gooJobSticker.SetActive(true);
                yield return new WaitForSeconds(8f);
                coinFlip.gar.LookAtPlayer();
                coinFlip.UnlockCheevo("ENDING_10FLIP");
                yield return new WaitForSeconds(3f);
                coinFlip.panelManager.SetPanelArrangement(5);
            }
            else
            {
                while (coinFlip.currentFlipDuration < (double) flipTime)
                {
                    coinFlip.currentFlipDuration += Time.deltaTime;
                    coinFlip.currentFlipAnimateDuration += Time.deltaTime * 360f / flipTime * flipTurnMultiplier;
                    coinFlip.transform.eulerAngles = new Vector3(0.0f, 0.0f, coinFlip.currentFlipAnimateDuration);
                    yield return null;
                }
                coinFlip.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                var money = SaveManager.SaveData.CoinValue * new BigInteger(Math.Ceiling(Math.Pow(SaveManager.SaveData.ComboMult, coinFlip.headsComboNum - 1)));
                if (SaveManager.SaveData.QueuedPennyTraps > 0)
                {
                    SaveManager.SaveData.QueuedPennyTraps--;
                    money = 1 * new BigInteger(Math.Ceiling(Math.Pow(SaveManager.SaveData.ComboMult, coinFlip.headsComboNum - 1)));
                }
                if (SaveManager.SaveData.QueuedTaxTraps > 0)
                {
                    SaveManager.SaveData.QueuedTaxTraps--;
                    money *= -1;
                }
                if (heads)
                {
                    var message = "HEADS";
                    for (var index = 1; index < coinFlip.headsComboNum; ++index)
                        message += "!";
                    UnfairFlipsAPMod.ArchipelagoHandler.CheckLocation(0x100 + coinFlip.headsComboNum);
                    coinFlip.messageManager.ShowMessage(message);
                    coinFlip.img.sprite = coinFlip.coinColors[coinFlip.currentCoin].coinHappy;
                    SaveManager.SaveData.PlayerMoney += money;
                    coinFlip.UnlockCheevo(coinFlip.headsComboNum + "FLIP");
                    var gameObject = Object.Instantiate(coinFlip.prf_sfx);
                    gameObject.GetComponent<AudioSource>().clip = coinFlip.headsSounds[Math.Min(coinFlip.headsComboNum - 1, 9)];
                    gameObject.GetComponent<AudioSource>().Play();
                    Object.Destroy(gameObject, 3f);
                }
                else
                {
                    var str = "<color=#FFFFFF77>TAILS";
                    if (comboFailed)
                        str += "...";
                    var message = str + "</color>";
                    coinFlip.messageManager.ShowMessage(message);
                    coinFlip.img.sprite = coinFlip.coinColors[coinFlip.currentCoin].coinSad;
                    coinFlip.audioSource.clip = coinFlip.landingSounds[Random.Range(0, coinFlip.landingSounds.Length)];
                    coinFlip.audioSource.volume = 0.5f;
                    coinFlip.audioSource.Play();
                }
                _flipMoneyResult = money;
                FindObjectOfType<FlipResultMessage>().ShowResult(heads, 0, coinFlip.headsComboNum);
                if (coinFlip.numFlips > 2 && coinFlip.panelManager.GetCurrentArrangement() < 1)
                    coinFlip.panelManager.SetPanelArrangement(1);
                if ((coinFlip.numFlips >= 8 || coinFlip.bigMoment & comboFailed) && (coinFlip.panelManager.GetCurrentArrangement() < 2 || coinFlip.bigMoment & comboFailed))
                {
                    coinFlip.panelManager.SetPanelArrangement(2);
                    if (coinFlip.bigMoment & comboFailed)
                        coinFlip.timeSinceMoved = 0.0f;
                    coinFlip.bigMoment = false;
                }
                if (coinFlip.headsComboNum == SlotData.RequiredHeads - 1)
                {
                    coinFlip.bigMoment = true;
                    coinFlip.panelManager.SetPanelArrangement(3);
                    coinFlip.timeSinceMoved = 0.0f;
                }
                coinFlip.prevWasHeads = heads;
                coinFlip.isFlipping = false;
                SaveManager.SaveGame();
                QueueNextAutoFlip();
            }
        }
    }
    
    public static void QueueNextAutoFlip()
    {
        if (!UnfairFlipsAPMod.SaveDataHandler.SaveData.HasAutoFlip)
            return;
        if (!AutoFlipIconHandler.IsAutoFlipEnabled)
            return;
        _queuedAutoFlip = _coinFlip.StartCoroutine(AutoFlipDelayCoroutine(_coinFlip));
    }
    
    private static IEnumerator AutoFlipDelayCoroutine(CoinFlip coinFlip)
    {
        var delay = UnfairFlipsAPMod.SaveDataHandler.SaveData.AutoFlipAddition;
        if (delay > 0f)
            yield return new WaitForSeconds(delay);
        if (!coinFlip.isFlipping 
            && UnfairFlipsAPMod.SaveDataHandler.SaveData.HasAutoFlip 
            && AutoFlipIconHandler.IsAutoFlipEnabled)
            coinFlip.DoFlip();
    }

    public void Kill()
    {
        var coinFlip = FindObjectOfType<CoinFlip>();
        if (coinFlip != null)
            coinFlip.headsComboNum = 0;
    }
}
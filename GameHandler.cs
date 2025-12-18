using System;
using System.Collections;
using HarmonyLib;
using Tweens;
using Tweens.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace UnfairFlipsAPMod;

public class GameHandler
{
    [HarmonyPatch(typeof(CoinFlip))]
    private class CoinFlip_Patch
    {
        private static SaveDataHandler SaveManager => UnfairFlipsAPMod.SaveDataHandler;
        private static SlotData SlotData => UnfairFlipsAPMod.SlotData;
        
        [HarmonyPatch("DoFlip")]
        [HarmonyPrefix]
        private static bool DoFlip_Prefix(CoinFlip __instance)
        {
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
                coinFlip.audioSource.clip = coinFlip.headsSounds[9];
                coinFlip.audioSource.Play();
                Object.FindObjectOfType<FlipResultMessage>().ShowResult(true, 0L, 10);
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
                long money = SaveManager.SaveData.CoinValue * Mathf.CeilToInt(Mathf.Pow(SaveManager.SaveData.ComboMult, coinFlip.headsComboNum - 1));
                if (SaveManager.SaveData.QueuedPennyTraps > 0)
                {
                    SaveManager.SaveData.QueuedPennyTraps--;
                    money = 1 * Mathf.CeilToInt(Mathf.Pow(SaveManager.SaveData.ComboMult, coinFlip.headsComboNum - 1));
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
                Object.FindObjectOfType<FlipResultMessage>().ShowResult(heads, money, coinFlip.headsComboNum);
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
            }
        }
    }

    public void Kill()
    {
        var coinFlip = Object.FindObjectOfType<CoinFlip>();
        if (coinFlip != null)
            coinFlip.headsComboNum = 0;
    }
}
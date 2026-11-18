using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PourController : MonoBehaviour
{
    public static PourController Instance { get; private set; }

    [Header("Cài đặt Game")]
    public float liftOffset = 0.5f;
    public float moveSpeed = 0.1f;
    public float pourAngle = 90f;

    [Header("Hiệu ứng")]
    public LineRenderer waterStream;
    public ParticleSystem waterSplashPrefab;
    public ParticleSystem bottleDonePrefab;

    [Header("Danh sách chai nước")]
    public List<Bottle> allBottles;

    private List<Bottle> busyBottles = new List<Bottle>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void DropBottle(Bottle bottle)
    {
        Vector3 groundPos = bottle.transform.position - new Vector3(0f, liftOffset, 0f);
        if (AudioManager.instance != null) AudioManager.instance.PlayBottleDown();
        StartCoroutine(AnimateBottle(bottle.transform, groundPos, 0f, moveSpeed));
    }

    public void LiftUpBottle(Bottle bottle)
    {
        if (AudioManager.instance != null) AudioManager.instance.PlayBottleLift();
        Vector3 liftPos = bottle.transform.position + new Vector3(0f, liftOffset, 0f);
        StartCoroutine(AnimateBottle(bottle.transform, liftPos, 0f, moveSpeed));
    }

    public void ExecutePour(Bottle source, Bottle target)
    {
        Vector3 sourceGroundPos = source.transform.position - new Vector3(0f, liftOffset, 0f);
        StartCoroutine(PourWaterRoutine(source, target, sourceGroundPos));
    }

    public IEnumerator PourWaterRoutine(Bottle source, Bottle target, Vector3 groundPos)
    {
        if (target.isFull() || source.isEmpty() || (!target.isEmpty() && target.getTopColor().Peek() != source.getTopColor().Peek()))
        {
            yield return StartCoroutine(AnimateBottle(source.transform, groundPos, 0f, moveSpeed));
            yield break;
        }

        busyBottles.Add(source);
        busyBottles.Add(target);

        float direction = Mathf.Sign(target.transform.position.x - source.transform.position.x);
        float targetAngle = direction > 0 ? -pourAngle : pourAngle;
        Vector3 pourPosition = target.transform.position + new Vector3(-direction * source.pourOffsetX, source.pourOffsetY, 0f);
        yield return StartCoroutine(AnimateBottle(source.transform, pourPosition, targetAngle, moveSpeed));

        Stack<WaterColor> colorStack = source.getTopColor();
        int amountToPour = colorStack.Count;
        int spaceInTarget = target.capacity - target.currentWaterCount;
        int actualPourAmount = Mathf.Min(amountToPour, spaceInTarget);

        if (actualPourAmount > 0)
        {
            Color unityColor = source.GetUnityColor(colorStack.Peek());

            int srcStartCount = source.currentWaterCount;
            int tgtStartCount = target.currentWaterCount;
            int srcEndCount = srcStartCount - actualPourAmount;
            int tgtEndCount = tgtStartCount + actualPourAmount;

            Vector3[] tgtOrigScales = new Vector3[actualPourAmount];
            Vector3[] srcOrigScales = new Vector3[actualPourAmount];

            for (int i = 0; i < actualPourAmount; i++)
            {
                var tgtRend = target.waterLayerRenderers[tgtStartCount + i];
                tgtOrigScales[i] = tgtRend.transform.localScale;
                tgtRend.gameObject.SetActive(true);
                tgtRend.color = unityColor;
                tgtRend.transform.localScale = new Vector3(tgtOrigScales[i].x, 0f, tgtOrigScales[i].z);

                var srcRend = source.waterLayerRenderers[srcStartCount - 1 - i];
                srcOrigScales[i] = srcRend.transform.localScale;
            }

            Vector3 splashPos = target.ovalInsideRenderer.transform.position;
            if (waterStream != null)
            {
                waterStream.gameObject.SetActive(true);
                waterStream.startColor = unityColor;
                waterStream.endColor = unityColor;
            }

            ParticleSystem splash = null;
            if (waterSplashPrefab != null)
            {
                splash = Instantiate(waterSplashPrefab, splashPos, Quaternion.identity);
                var mainModule = splash.main;
                mainModule.startColor = unityColor;
                splash.Play();
            }

            float timePerLayer = 0.25f;
            float pourDuration = actualPourAmount * timePerLayer;
            float timePassed = 0f;

            if (AudioManager.instance != null) AudioManager.instance.StartPourSound();

            while (timePassed < pourDuration)
            {
                timePassed += Time.deltaTime;
                float percent = timePassed / pourDuration;
                float totalProgress = percent * actualPourAmount;

                for (int i = 0; i < actualPourAmount; i++)
                {
                    float layerProgress = Mathf.Clamp01(totalProgress - i);

                    var srcRend = source.waterLayerRenderers[srcStartCount - 1 - i];
                    srcRend.transform.localScale = new Vector3(srcOrigScales[i].x, Mathf.Lerp(srcOrigScales[i].y, 0f, layerProgress), srcOrigScales[i].z);

                    var tgtRend = target.waterLayerRenderers[tgtStartCount + i];
                    tgtRend.transform.localScale = new Vector3(tgtOrigScales[i].x, Mathf.Lerp(0f, tgtOrigScales[i].y, layerProgress), tgtOrigScales[i].z);
                }

                float srcCurrentY = Mathf.Lerp(source.GetOvalYPosition(srcStartCount), source.GetOvalYPosition(srcEndCount), percent);
                float tgtCurrentY = Mathf.Lerp(target.GetOvalYPosition(tgtStartCount), target.GetOvalYPosition(tgtEndCount), percent);

                Vector3 srcPos = source.ovalInsideRenderer.transform.parent.localPosition;
                srcPos.y = srcCurrentY;
                source.ovalInsideRenderer.transform.parent.localPosition = srcPos;

                Vector3 tgtPos = target.ovalInsideRenderer.transform.parent.localPosition;
                tgtPos.y = tgtCurrentY;
                target.ovalInsideRenderer.transform.parent.localPosition = tgtPos;

                if (waterStream != null)
                {
                    waterStream.SetPosition(0, source.mouthPoint.position);
                    waterStream.SetPosition(1, target.ovalInsideRenderer.transform.position);
                }
                if (splash != null) splash.transform.position = target.ovalInsideRenderer.transform.position;

                yield return null;
            }

            if (AudioManager.instance != null) AudioManager.instance.StopPourSound();

            if (splash != null)
            {
                splash.Stop();
                Destroy(splash.gameObject, 1.5f);
            }

            for (int i = 0; i < actualPourAmount; i++)
            {
                source.waterLayerRenderers[srcStartCount - 1 - i].transform.localScale = srcOrigScales[i];
                target.waterLayerRenderers[tgtStartCount + i].transform.localScale = tgtOrigScales[i];
            }

            int poured = target.addNewColor(colorStack);
            source.removeTopColor(poured);

            source.updateBottleVisuals();
            target.updateBottleVisuals();

            if (UndoManager.Instance != null)
            {
                UndoManager.Instance.PushStep(source, target, poured);
            }
        }

        if (waterStream != null) waterStream.gameObject.SetActive(false);

        yield return StartCoroutine(AnimateBottle(source.transform, groundPos, 0f, moveSpeed));

        busyBottles.Remove(source);
        busyBottles.Remove(target);

        if (target.isCompleted())
        {
            Debug.Log("Chai này đã hoàn thiện");
            if (bottleDonePrefab != null) Instantiate(bottleDonePrefab, target.mouthPoint.position, Quaternion.identity);
            target.CloseCork();
        }

        if (GameManager.instance != null)
        {
            bool isWin = GameManager.instance.CheckWin();
            if (!isWin) GameManager.instance.CheckGameState();
        }
    }

    public IEnumerator AnimateBottle(Transform bottleTransform, Vector3 targetPos, float targetRotation, float duration)
    {
        Vector3 startPos = bottleTransform.position;
        Quaternion startRot = bottleTransform.rotation;
        Quaternion endRot = Quaternion.Euler(0, 0, targetRotation);

        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float percent = timePassed / duration;

            bottleTransform.position = Vector3.Lerp(startPos, targetPos, percent);
            bottleTransform.rotation = Quaternion.Lerp(startRot, endRot, percent);

            yield return null;
        }

        bottleTransform.position = targetPos;
        bottleTransform.rotation = endRot;
    }

    public bool HasAnyValidMove()
    {
        for (int i = 0; i < allBottles.Count; i++)
        {
            Bottle fromBottle = allBottles[i];
            if (fromBottle.isEmpty() || fromBottle.isCompleted()) continue;
            WaterColor colorToPour = fromBottle.getTopColor().Peek();

            for (int j = 0; j < allBottles.Count; j++)
            {
                if (i == j) continue;

                Bottle toBottle = allBottles[j];
                if (toBottle.isFull()) continue;
                if (toBottle.isEmpty() || toBottle.getTopColor().Peek() == colorToPour)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ExecuteHintPour(Bottle source, Bottle target)
    {
        if (busyBottles.Contains(source) || busyBottles.Contains(target)) return;

        Vector3 groundPos = source.transform.position;
        StartCoroutine(PourWaterRoutine(source, target, groundPos));
    }

    public IEnumerator RearrangeBottlesRoutine(LevelManager levelManager)
    {
        List<Vector3> targetPositions = levelManager.GetBottleTargetPositions(allBottles.Count);
        List<Vector3> startPositions = new List<Vector3>();
        for (int i = 0; i < allBottles.Count; i++)
        {
            startPositions.Add(allBottles[i].transform.position);
        }

        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            for (int i = 0; i < allBottles.Count; i++)
            {
                if (i < targetPositions.Count)
                {
                    allBottles[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], easeT);
                }
            }

            yield return null;
        }

        for (int i = 0; i < allBottles.Count; i++)
        {
            if (i < targetPositions.Count)
            {
                allBottles[i].transform.position = targetPositions[i];
            }
        }
    }

    public bool IsBusy()
    {
        return busyBottles.Count > 0;
    }

    public bool IsBusyWith(Bottle bottle)
    {
        return busyBottles.Contains(bottle);
    }
}

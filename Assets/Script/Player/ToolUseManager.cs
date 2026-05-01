using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolUseManager : MonoBehaviour
{
    [SerializeField] private float useToolAnimationPauseDuration = 0.5f;
    [SerializeField] private float liftToolAnimationPauseDuration = 0.5f;
    [SerializeField] private float afterLiftToolAnimationPauseDuration = 0.5f;

    private WaitForSeconds useToolAnimationPause;
    private WaitForSeconds liftToolAnimationPause;
    private WaitForSeconds afterLiftToolAnimationPause;

    private CharaAttribute toolsCharacterAttribute;
    private List<CharaAttribute> charaAttributeCustomisationList;
    private AnimatorOverrideControls overrideControls;
    private GridPropertiesManager gridPropertiesManager;

    private void Awake()
    {
        toolsCharacterAttribute = new CharaAttribute(CharacterPartAnimator.tool, PartVariantColour.none, PartVariantType.hoe);
        charaAttributeCustomisationList = new List<CharaAttribute>();
        overrideControls = GetComponentInChildren<AnimatorOverrideControls>();
        gridPropertiesManager = FindObjectOfType<GridPropertiesManager>();
    }

    private void Start()
    {
        useToolAnimationPause = new WaitForSeconds(useToolAnimationPauseDuration);
        liftToolAnimationPause = new WaitForSeconds(liftToolAnimationPauseDuration);
        afterLiftToolAnimationPause = new WaitForSeconds(afterLiftToolAnimationPauseDuration);
    }

    public void HoeGround(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails, System.Action onComplete)
    {
        StartCoroutine(HoeGroundRoutine(playerDirection, gridPropertyDetails, onComplete));
    }

    private IEnumerator HoeGroundRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails, System.Action onComplete)
    {
        toolsCharacterAttribute.partVariantType = PartVariantType.hoe;
        charaAttributeCustomisationList.Clear();
        charaAttributeCustomisationList.Add(toolsCharacterAttribute);
        overrideControls.ApplyCharacterCustomisationPatameters(charaAttributeCustomisationList);

        TriggerUseToolAnimation(playerDirection);

        yield return useToolAnimationPause;

        if (gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0;
        }
        gridPropertiesManager.SetGridDetials(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);
        gridPropertiesManager.DisplayDugGround(gridPropertyDetails);

        onComplete?.Invoke();
    }

    public void WaterGround(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails, System.Action onComplete)
    {
        StartCoroutine(WaterGroundRoutine(playerDirection, gridPropertyDetails, onComplete));
    }

    private IEnumerator WaterGroundRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails, System.Action onComplete)
    {
        toolsCharacterAttribute.partVariantType = PartVariantType.wateringCan;
        charaAttributeCustomisationList.Clear();
        charaAttributeCustomisationList.Add(toolsCharacterAttribute);
        overrideControls.ApplyCharacterCustomisationPatameters(charaAttributeCustomisationList);

        TriggerLiftToolAnimation(playerDirection);

        yield return liftToolAnimationPause;

        if (gridPropertyDetails.daysSinceWatered == -1)
        {
            gridPropertyDetails.daysSinceWatered = 0;
        }
        gridPropertiesManager.SetGridDetials(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);
        gridPropertiesManager.DisplayDugGround(gridPropertyDetails);

        yield return afterLiftToolAnimationPause;

        onComplete?.Invoke();
    }

    private void TriggerUseToolAnimation(Vector3Int direction)
    {
        if (direction == Vector3Int.right)
        {
            PlayerManager.Instance.SetUsingToolRight(true);
        }
        else if (direction == Vector3Int.left)
        {
            PlayerManager.Instance.SetUsingToolLeft(true);
        }
        else if (direction == Vector3Int.up)
        {
            PlayerManager.Instance.SetUsingToolUp(true);
        }
        else if (direction == Vector3Int.down)
        {
            PlayerManager.Instance.SetUsingToolDown(true);
        }
    }

    private void TriggerLiftToolAnimation(Vector3Int direction)
    {
        if (direction == Vector3Int.right)
        {
            PlayerManager.Instance.SetLiftingToolRight(true);
        }
        else if (direction == Vector3Int.left)
        {
            PlayerManager.Instance.SetLiftingToolLeft(true);
        }
        else if (direction == Vector3Int.up)
        {
            PlayerManager.Instance.SetLiftingToolUp(true);
        }
        else if (direction == Vector3Int.down)
        {
            PlayerManager.Instance.SetLiftingToolDown(true);
        }
    }
}

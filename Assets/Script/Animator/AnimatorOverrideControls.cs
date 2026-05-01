using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorOverrideControls : MonoBehaviour
{
    [SerializeField] private GameObject character = null;
    [SerializeField] private ScriptObjectAnimator[] scriptObjectAnimatorList = null;

    private Dictionary<AnimationClip, ScriptObjectAnimator> animationTypeDictionaryByAnmation;
    private Dictionary<string, ScriptObjectAnimator> animationTypeDictionaryByCompositeAttributeKey;

    private void Start()
    {
        animationTypeDictionaryByAnmation = new Dictionary<AnimationClip, ScriptObjectAnimator>();
        foreach(ScriptObjectAnimator item in scriptObjectAnimatorList)
        {
            animationTypeDictionaryByAnmation.Add(item.animationClip, item);
        }
        animationTypeDictionaryByCompositeAttributeKey = new Dictionary<string, ScriptObjectAnimator>();
        foreach(ScriptObjectAnimator item in scriptObjectAnimatorList)
        {
            string key = item.characterPart.ToString() + item.partVariantColour.ToString()
                + item.partVariantType.ToString() + item.animationName.ToString();
            animationTypeDictionaryByCompositeAttributeKey.Add(key, item);
        }
    }
    public void ApplyCharacterCustomisationPatameters(List<CharaAttribute> charaAttributeList)
    {
        //Debug.Log("ApplyCharacterCustomisationPatameters");
        foreach(CharaAttribute charaAttribute in charaAttributeList)
        {
            Animator currentAnimator = null;
            List<KeyValuePair<AnimationClip, AnimationClip>> animKeyValuePairList = new 
                List<KeyValuePair<AnimationClip, AnimationClip>>();
            string animatorSOAssestName = charaAttribute.characterPartAnimator.ToString();
            //Debug.Log("animatorSOAssestName: " + animatorSOAssestName);
            Animator[] animatorArray = character.GetComponentsInChildren<Animator>();
            for(int i = 0; i < animatorArray.Length; i++)
            {
                if(animatorArray[i].name == animatorSOAssestName)
                {
                    currentAnimator = animatorArray[i];
                    break;
                }
            }

            if(currentAnimator == null)
            {
                //Debug.LogError("未找到 Animator: " + animatorSOAssestName);
                continue;
            }

            AnimatorOverrideController aoc = new AnimatorOverrideController(currentAnimator.runtimeAnimatorController);
            List<AnimationClip> animationList = new List<AnimationClip>(aoc.animationClips);

            foreach(AnimationClip animationClip in animationList)
            {
                ScriptObjectAnimator scriptObjectAnimator = null;
                bool foundInDict = animationTypeDictionaryByAnmation.TryGetValue(animationClip, out scriptObjectAnimator);
                if (foundInDict)
                {
                    string key = charaAttribute.characterPartAnimator.ToString() + 
                        charaAttribute.partVariantColour.ToString() +
                        charaAttribute.partVariantType.ToString() + 
                        scriptObjectAnimator.animationName.ToString();
                    ScriptObjectAnimator swapScriptObjectAnimator = null;
                    bool swapFoundInDict = animationTypeDictionaryByCompositeAttributeKey.TryGetValue(key, out swapScriptObjectAnimator);
                    if (swapFoundInDict)
                    {
                        //Debug.Log("替换值：" + swapScriptObjectAnimator.animationClip.name);
                        AnimationClip swapAnimationClip = swapScriptObjectAnimator.animationClip;
                        animKeyValuePairList.Add(new KeyValuePair<AnimationClip, AnimationClip>(animationClip, swapAnimationClip));
                    }
                }
            }
            aoc.ApplyOverrides(animKeyValuePairList);
            currentAnimator.runtimeAnimatorController = aoc;
            //Debug.Log("应用动画覆盖后的动画控制器，然后重新赋值动画控制器");
        }
    }
}

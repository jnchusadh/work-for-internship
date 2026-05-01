[System.Serializable]
public struct CharaAttribute
{
    public CharacterPartAnimator characterPartAnimator;
    public PartVariantColour partVariantColour;
    public PartVariantType partVariantType;
    public CharaAttribute(CharacterPartAnimator characterPartAnimator,PartVariantColour partVariantColour, PartVariantType partVariantType)
    {
        this.characterPartAnimator = characterPartAnimator;
        this.partVariantColour = partVariantColour;
        this.partVariantType = partVariantType;
    }
}

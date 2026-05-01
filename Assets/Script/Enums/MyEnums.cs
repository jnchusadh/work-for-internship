public enum ToolEffect
{
    none,
    waterin,
}
public enum Direction
{
    left,
    right,
    up,
    down,
}
public enum ItemType
{
    Seed,
    Commodity,
    Watering_tool,
    Hoeing_tool,
    Collecting_tool,
    none,
    count
}
public enum InventoryLocation
{
    player,
    chest,
    count
}
public enum Season
{
    spring,
    summer,
    autumn,
    winter,
    none,
    count
}
public enum SceneName
{
    Scene1_Farm,
    Scene2_Field,
    Scene3_Cabin,
}
public enum AnimationName
{
    idleDown,
    idleUp,
    idleRight,
    idleLeft,
    walkDown,
    walkUp,
    walkRight,
    walkLeft,
    useToolDown,
    useToolUp,
    useToolRight,
    useToolLeft,
    count 
}
public enum CharacterPartAnimator
{
    body,
    arms,
    hat,
    hair,
    tool,
    count
}
public enum PartVariantColour
{
    none,
    count
}
public enum PartVariantType
{
    none,
    carry,
    hoe,
    wateringCan,
    count
}
public enum GridBoolProperty
{
    diggable,
    canDropItem,
    canPlaceFurniture,
    isPath,
    isNPCObstacle
}

public enum SceneObjectState
{
    store = 0,
    binding = 1,
    placed = 2
}

public enum Timing
{
    start = 0,
    custom = 1
}

public enum Vowel
{
    A = 0, // 0x0
    I = 1, // 0x0
    U = 2, // 0x0
    E = 3, // 0x0
    O = 4, // 0x0
    N = 5, // 0x0
}

public enum eVowel
{
    A = 0, // 0x0
    I = 1, // 0x0
    U = 2, // 0x0
    E = 3, // 0x0
    O = 4, // 0x0
    N = 5, // 0x0
}


public enum eFacialParts
{
    BROW = 1,
    EYE = 2,
    MOUTH = 3,
    MOUTH_OPEN = 4,
    MOUTH_HALFCLOSE = 5,
    MOUTH_CLOSE = 6,
    EYE_BLINK = 7,
    CHEEK = 8,
    FOREHEAD_SHADE = 9,
    EYE_HIGHLIGHT = 10,
    EYE_SCALE = 11,
    UNKNOWN = int.MaxValue // 4294967295
}

public enum eFaceExpression
{
    USUALLY = 1, // 0x0
    ANGRY = 2, // 0x0
    SAD = 3, // 0x0
    HAPPY = 4, // 0x0
    THINK = 5, // 0x0
    SURPRISE = 6, // 0x0
    SERIOUS = 7, // 0x0
    SHY = 8, // 0x0
    BAD = 9, // 0x0
    BITTER_SMILE = 10, // 0x0
    DOYA = 11, // 0x0
    DELSION = 12, // 0x0
    RELAX = 13, // 0x0
    ANIXUETY = 14, // 0x0
    PANIC = 15, // 0x0
    SERIOUSLY = 16, // 0x0
    AMAZEED = 17, // 0x0
    WORRY = 18, // 0x0
    EMBARRASSED = 19, // 0x0
    SHY_SMILE = 20, // 0x0
    EMBARRASSED_SMILE = 21, // 0x0
    ANXIETY = 22, // 0x0
    HAPPINESS = 23, // 0x0
    TRY_HARD = 24, // 0x0
    LOUR = 25, // 0x0
    LOOKS_VERY_SORRY = 26, // 0x0
    CRY_WITH_JOY = 27, // 0x0
    SORROWFUL = 28, // 0x0
    NERVOUS = 29, // 0x0
    FORCED_SMILE = 30, // 0x0
    CRY = 31, // 0x0
    PAIN = 32, // 0x0
    INDIGNANT = 33, // 0x0
    EARNEST = 34, // 0x0
    VAINLY_ATTEMPT2 = 35, // 0x0
    FLUSTERED = 36, // 0x0
    FRANTIC = 37, // 0x0
    ASTONISHMENT = 38, // 0x0
    THINKING_SERIOUSLY = 39, // 0x0
    BLUSHING_BUT_COLD = 40, // 0x0
    LISTLESS = 41, // 0x0
    ANIXUETY2 = 42, // 0x0
    WINK = 43, // 0x0
    RELAX2 = 44, // 0x0
    IMMERSION = 45, // 0x0
    SQUEEZE_YOUR_EYES_WITHOUT_LAUGHING = 46, // 0x0
    WAIL = 47, // 0x0
    CLOSE_YOUR_EYES_AND_CRY = 48, // 0x0
    BE_SCARED = 49, // 0x0
    SOME_HELPLESS = 50, // 0x0
    LOSE_ONES_MIND = 51, // 0x0
    MYSTERIOUS = 52, // 0x0
    BE_EXCITED2 = 53, // 0x0
    CHUUNIBYOU = 54, // 0x0
    CHUUNIBYOU_2 = 55, // 0x0
    LAUGH = 56, // 0x0
    PRIMNESS = 57, // 0x0
    ANGRY1 = 58, // 0x0
    LANGUOR = 59, // 0x0
    SMILE_ALL = 97, // 0x0
    PANIC_ALL = 98, // 0x0
    SHY_ALL = 99, // 0x0
    DOYA_ALL = 100, // 0x0
    USUALLY2 = 101, // 0x0
    ANGRY2 = 102, // 0x0
    SAD2 = 103, // 0x0
    HAPPY2 = 104, // 0x0
    THINK2 = 105, // 0x0
    SURPRISE2 = 106, // 0x0
    SERIOUS2 = 107, // 0x0
    SHY2 = 108, // 0x0
    BAD2 = 109, // 0x0
    BITTER_SMILE2 = 110, // 0x0
    DOYA2 = 111, // 0x0
    LAZY = 198, // 0x0
    MOE = 199, // 0x0
    blink = 200, // 0x0
    USUALLY3 = 201, // 0x0
    ANGRY3 = 202, // 0x0
    SAD3 = 203, // 0x0
    HAPPY3 = 204, // 0x0
    THINK3 = 205, // 0x0
    SURPRISE3 = 206, // 0x0
    SERIOUS3 = 207, // 0x0
    SHY3 = 208, // 0x0
    BAD3 = 209, // 0x0
    BITTER_SMILE3 = 210, // 0x0
    DOYA3 = 211, // 0x0
    wink2 = 212, // 0x0
    happy4 = 213, // 0x0
    happy5 = 214, // 0x0
    openmouse = 215, // 0x0
    openmouse2 = 216, // 0x0
    tired = 217, // 0x0
    what = 218, // 0x0
    openeye = 219, // 0x0
    UNKNOWN = 999, // 0x0
}

public enum SpringColliderID
{
    // Fields
    None = 0, // 0x0
    Hips = 1, // 0x0
    Spine = 2, // 0x0
    LeftUpLeg = 3, // 0x0
    LeftLeg = 4, // 0x0
    LeftFoot = 5, // 0x0
    RightUpLeg = 6, // 0x0
    RightLeg = 7, // 0x0
    RightFoot = 8, // 0x0
    LeftShoulder = 9, // 0x0
    LeftArm = 10, // 0x0
    LeftForeArm = 11, // 0x0
    LeftHand = 12, // 0x0
    Neck = 13, // 0x0
    Head_End = 14, // 0x0
    RightShoulder = 15, // 0x0
    RightArm = 16, // 0x0
    RightForeArm = 17, // 0x0
    RightHand = 18, // 0x0
    Spine1 = 19, // 0x0
    Bip001_Pelvis = 20, // 0x0
    Bip001_Spine = 21, // 0x0
    Bip001_L_Thigh = 22, // 0x0
    Bip001_L_Calf = 23, // 0x0
    Bip001_L_Foot = 24, // 0x0
    Bip001_R_Thigh = 25, // 0x0
    Bip001_R_Calf = 26, // 0x0
    Bip001_R_Foot = 27, // 0x0
    Bip001_L_Clavicle = 28, // 0x0
    Bip001_L_UpperArm = 29, // 0x0
    Bip001_L_Forearm = 30, // 0x0
    Bip001_L_Hand = 31, // 0x0
    Bip001_Neck = 32, // 0x0
    Head_Point = 33, // 0x0
    Bip001_R_Clavicle = 34, // 0x0
    Bip001_R_UpperArm = 35, // 0x0
    Bip001_R_Forearm = 36, // 0x0
    Bip001_R_Hand = 37, // 0x0
    Bip001_Spine1 = 38, // 0x0
}

public enum eAvatarType
{
    Attach = 0, // 0x0
    Skinning = 1, // 0x0
}

public enum AnimationIndex
{
    None = 0, // 0x0
    A = 1, // 0x0
    I = 2, // 0x0
    U = 3, // 0x0
    E = 4, // 0x0
    O = 5, // 0x0
    N = 6, // 0x0
}
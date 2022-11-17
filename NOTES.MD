### 写给自己看的备忘

### 角色的human.bdl
+ FacialSettings 表情设置  
  > blink相关字段控制眨眼  
  > 各个表情里的blendData对应了face和brow两部分的blendShapes  
  > cheekPower和foreheadShade的控制在shader里  
+ VowelSetting 口型设置
  > 就是日语里 あいうえお 这五个元音加个 ん 的口型  
  > shapeIndex是直接算face上的所有blendShape的索引位置，不只是mouth的  
+ EyeSettings 眼球可动范围设置
  > 没用到...LookAt在face上没生效,不知道原因  
+ ScaleSettings 尺寸设置
  > scaleRatio控制角色整体大小  
  > isLoli 应该是用于判断载入通用模型时使用贫乳模型还是巨乳模型  
  > breast 相关字段控制通用模型的胸部尺寸微调，在通用模型的breast上的blendShape里控制  
  > listScaleData 应该是控制头部和脖子尺寸的，但是设置了之后反而有问题，暂时没用到  
+ SpringSetting 物理骨骼设置
  > 保存了bone和collider相关参数  
  > 应该是内部实现的一套物理系统，并不能和开源的实现匹配上  
  > 拿SpringBone和DynamicBone都试了一遍，效果都不好...  
  > 暂时解决不了  
+ AvatarSetting
  > 模型本身的信息,拿Prefab创建模型实例就可以了  
  > Type为Attach的需要根据Dummy字段绑定到对应骨骼节点上  
  > Type为Skinning的自带骨骼，绑定上动画就可以  
  > 部分饰品显示有问题...  
  > 对于身体的avatar，有一个HeelSettings，用于控制脚的旋转和整体模型高度位置调整  
+ 另外，角色动作用的Avatar在`actors/skeletons.bdl`里  
+ 角色脚下的阴影控制在`GeometricShadowCast`里,但是会跟着脚动..应该需要一个映射到地面的方法;没有实现,先注释掉了

### Dance Timeline
+ Char Animtion Group
  > 舞蹈动作的Track，AR版的只有一个;加载了多个角色就Clone
  > Live版的会依据该歌曲的人数有对应数量的动作Track,不过除\"Are Many Chance!!!\"外都是全员相同的
  > 把角色的Animator绑定上去就好了
+ Eyes Blink Group
  > 眨眼的Track，按一般逻辑自己实现了`EyeBlinkPlayableAsset`  
+ Expression Group
  > 表情Track，由于表情需要和口型及眨眼配合计算权重，有点复杂，没有做  
+ Lips Group 和 Vowel Group
  > 口型的Track，Ar版用的`LipsyncAsset`,live版用的`VowelPlayableAsset`  
  > 最后把两者结构统一了一下,具体控制的实现在`VowelManager`里  
+ CharLight Sub-Group
  > 角色灯光效果的Track,应该是通过控制材质和着色器参数改变角色的显示效果的  
  > 没有做实现  
+ Post Process Track
  > 后处理（光晕、景深、色调等等）,怎么实现完全没有头绪...放弃了  
+ RefControlTrack
  > 控制场景内部分物体启停的Track,如烟花效果之类的  
  > 简单实现了一版，但会有部分guid找不到的情况，没有完全搞清楚  
+ timeline上有一个`SceneTimelineRebinder`,用于将场景中对应的对象绑定到timeline track上,但会有部分对象找不到,这部分没有完全解明白  
+ 摄像机上有`VirtualCameraFollowAndLookAt`类,用于控制摄像机注视点的;部分注视节点的会找不到,有一部分(如HeadFix)按一般理解做了兼容,另一些就忽略了...  
+ 关于歌曲的第二段循环部分,没有找到记录了循环时间段的信息  
  > 目前以舞蹈动作轴Part2起始结束时间来做的计算;但部分歌曲正常部分会有断档  
  > 也许可以手工调整出正确的时间点,然后记录到配置json中?就是比较麻烦...  
  
### 场景
+ 依赖`Volumetric Light Beam`这个插件实现体积光;但根据解包出来的数据结构,游戏内用的应该是魔改了的版本,所以会有些问题  
+ 实现了`AutoRotation`来让四重异奏舞台的钟面动起来  
+ 场景配置`SceneEnvironment`  
  > 保存了场景的渲染风格、阴影配置、着色器配置、灯光配置、后处理配置等信息  
  > 大部分忽略了，只实现了摄像机镜头可视距离的部分  
+ 体积光光柱,会用到`AutoSingleLookAtConstraint`来控制光柱跟随角色  
+ 目前在Live里把角色放到了`TopLight`这个节点里,从实际效果看能让角色按正常显示和位移;但按命名看这应该只是控制灯光的,只是灯光位置刚好要和角色同步...
  > 没有找到其他控制角色位置的信息...
  > 在部分Live中有问题...比如在“禁忌の寓話”中会出点角色朝向不正确


### 数据库
+ ActorData: 人物/怪物 模型信息  
+ MotionControllerData: 动作动画controller数据信息  
+ FavorPose: Pose动作数据信息  

### 其他
+ MUSIC_0026(LiGHTs - プラネタリウム) 的SoundBank文件是不存在的，不过自己把这首歌的音频文件放进去能正常与AnimationClip匹配起来  
+ 0016,0021,0022,0023 这几个有动作的AnimationClip，但是没有Timeline，也许可以拿音乐匹配上去判断是哪边歌的舞蹈？不过口型数据肯定是没有了  

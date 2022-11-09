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
  > 没用到...LookAt在face上没生效  
+ ScaleSettings 尺寸设置
  > scaleRatio控制角色整体大小  
  > isLoli 应该是用于判断载入通用模型时使用贫乳模型还是巨乳模型  
  > breast 相关字段控制通用模型的胸部尺寸微调，在通用模型的breast上的blendShape里控制  
  > listScaleData 应该是控制头部和脖子尺寸的，但是设置了之后反而有问题，暂时没用到
+ SpringSetting 物理骨骼设置
  > 保存和bone和collider相关参数  
  > 应该是内部实现的一套物理系统，并不能和开源的实现匹配上  
  > 拿SpringBone和DynamicBone都试了一遍，效果都不好...  
  > 暂时解决不了  
+ AvatarSetting
  > 模型本身的信息，拿Prefab创建模型实例就可以了  
  > Type为Attach的需要根据Dummy字段绑定到对应骨骼节点上  
  > Type为Skinning的自带骨骼，绑定上动画就可以  
  > 对于身体的avatar，有一个HeelSettings，用于控制脚的旋转和整体模型高度位置调整  
+ 另外，角色动作用的Avatar在`actors/skeletons.bdl`里  
  
### Dance
+ Char Animtion Group
  > 舞蹈动作的Track，AR版的只有一个，应该要将各个`AvatarPart`的骨骼节点Merge之后绑上去的  
  > 实现上比较麻烦，目前使用的办法是把Track多复制几个，每个`AvatarPart`绑定一个，Timeline会自己控制同步  
+ Eyes Blink Group
  > 眨眼的Track，按一般逻辑自己实现了`EyeBlinkPlayableAsset`  
+ Expression Group
  > 表情Track，看了下解包，并没有表情变化，所以没管  
+ Lips Group
  > 口型的Track，自己实现了`LipsyncAsset`，具体的控制的实现在`VowelManager`里  
  
### 场景
+ 依赖`Volumetric Light Beam`这个插件实现体积光
+ 实现了`AutoRotation`来让四重异奏舞台的钟面动起来
  
### 数据库
+ ActorData: 人物/怪物 模型信息  
+ MotionControllerData: 动作动画controller数据信息  
+ FavorPose: Pose动作数据信息  

### 其他
+ MUSIC_0026(LiGHTs - プラネタリウム) 的SoundBank文件是不存在的，不过自己把这首歌的音频文件放进去能正常与AnimationClip匹配起来  
+ 0016,0021,0022,0023 这几个有动作的AnimationClip，但是没有Timeline，也许可以拿音乐匹配上去判断是哪边歌的舞蹈？不过口型数据肯定是没有了  

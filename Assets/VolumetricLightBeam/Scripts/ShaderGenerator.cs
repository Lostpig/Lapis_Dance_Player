#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace VLB
{
    public class ShaderGenerator : ScriptableObject
    {
        [SerializeField] TextAsset m_Base = null;
        TextAsset textAssetBase { get { return m_Base; } }

        [SerializeField] TextAsset m_Pass = null;
        TextAsset textAssetPass { get { return m_Pass; } }

        [SerializeField] TextAsset m_IncludesBuiltin = null;
        TextAsset textAssetIncludesBuiltin { get { return m_IncludesBuiltin; } }

        [SerializeField] TextAsset m_IncludesURP = null;
        TextAsset textAssetIncludesURP { get { return m_IncludesURP; } }

        [SerializeField] TextAsset m_IncludesHDRP = null;
        TextAsset textAssetIncludesHDRP { get { return m_IncludesHDRP; } }

        const string kShaderAssetName = "VLBGeneratedShader";

        static string GetOutputPath()
        {
            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Instance));
            if (string.IsNullOrEmpty(path)) path = "Plugins/VolumetricLightBeam/Shaders/";
            path = path.Replace("\\", "/");
            const string kAssetsFolder = "Assets/";
            if (path.IndexOf(kAssetsFolder) == 0) path = path.Remove(0, kAssetsFolder.Length);
            return path;
        }

        public static Shader Generate(RenderPipeline rp, RenderingMode rm, bool ditheringEnabled)
        {
            // The instance might not be accessible yet, when called from Config.OnEnable for instance if the Config is enabled before the ShaderGenerator.
            // In this case we don't generate the shader right away, we store the parameters instead and we'll generate the shader in ShaderGenerator.OnEnable.
            if (Instance == null)
            {
                ms_GenerationParamOnEnable = new GenerationParam { renderPipeline = rp, renderingMode = rm, ditheringEnabled = ditheringEnabled };
                return null;
            }

            return new GenShader(rp, rm, ditheringEnabled).Generate();
        }

        static string LoadText(TextAsset textAsset)
        {
            Debug.Assert(textAsset != null, "Fail to load a TextAsset, please try to reinstall the VolumetricLightBeam plugin");
            return textAsset.text;
        }

        TextAsset GetTextAssetIncludes(SRPHelper.RenderPipeline rp)
        {
            switch (rp)
            {
                case SRPHelper.RenderPipeline.BuiltIn:
                    return textAssetIncludesBuiltin;
                case SRPHelper.RenderPipeline.LWRP:
                case SRPHelper.RenderPipeline.URP:
                    return textAssetIncludesURP;
                case SRPHelper.RenderPipeline.HDRP:
                    return textAssetIncludesHDRP;
            }
            return null;
        }

        static bool IsFogSupported(SRPHelper.RenderPipeline rp) { return rp != SRPHelper.RenderPipeline.HDRP; }

        enum ShaderLangage { CG, HLSL }

        static ShaderLangage GetShaderLangage(SRPHelper.RenderPipeline rp)
        {
            switch (rp)
            {
                case SRPHelper.RenderPipeline.BuiltIn:
                case SRPHelper.RenderPipeline.LWRP:
                    return ShaderLangage.CG;
                case SRPHelper.RenderPipeline.URP:
                case SRPHelper.RenderPipeline.HDRP:
                    return ShaderLangage.HLSL;
            }
            return ShaderLangage.CG;
        }

        static string GetShaderLangagePre (ShaderLangage lang) { return lang == ShaderLangage.CG ? "CGPROGRAM" : "HLSLPROGRAM"; }
        static string GetShaderLangagePost(ShaderLangage lang) { return lang == ShaderLangage.CG ? "ENDCG" : "ENDHLSL"; }

        public class GenPass
        {
            CullMode m_CullMode;

            public GenPass(CullMode cullMode)
            {
                m_CullMode = cullMode;
            }

            public string Generate(SRPHelper.RenderPipeline rp, RenderingMode rm, bool ditheringEnabled, int passID, int passCount)
            {
                var code = LoadText(Instance.textAssetPass);

                code = code.Replace("{VLB_GEN_CULLING}", m_CullMode.ToString());
                code = code.Replace("{VLB_GEN_PRAGMA_INSTANCING}", rm == RenderingMode.GPUInstancing ? "#pragma multi_compile_instancing" : "");
                code = code.Replace("{VLB_GEN_PRAGMA_FOG}", IsFogSupported(rp) ? "#pragma multi_compile_fog" : "");

            #if UNITY_2019_1_OR_NEWER
                code = code.Replace("{VLB_GEN_PRAGMA_MULTI_COMPILE}", "#pragma multi_compile_local");
            #else
                code = code.Replace("{VLB_GEN_PRAGMA_MULTI_COMPILE}", "#pragma multi_compile");
            #endif

                var lang = GetShaderLangage(rp);
                code = code.Replace("{VLB_GEN_PROGRAM_PRE}", GetShaderLangagePre(lang));
                code = code.Replace("{VLB_GEN_PROGRAM_POST}", GetShaderLangagePost(lang));

                var passPre = "";

                if (passCount > 1)
                {
                    code = code.Replace("{VLB_GEN_INPUT_VS}", string.Format("{0}", passID));
                    code = code.Replace("{VLB_GEN_INPUT_FS}", string.Format("{0}", passID));
                }
                else
                {
                    code = code.Replace("{VLB_GEN_INPUT_VS}", string.Format("{0}", "v.texcoord.y"));
                    code = code.Replace("{VLB_GEN_INPUT_FS}", string.Format("{0}", "i.cameraPosObjectSpace_outsideBeam.w"));
                    passPre += "                #define VLB_PASS_OUTSIDEBEAM_FROM_VS_TO_FS 1" + System.Environment.NewLine;
                }

                if (rp != SRPHelper.RenderPipeline.BuiltIn)
                {
                    passPre += "                #define VLB_SRP_API 1" + System.Environment.NewLine;

                    if (rm == RenderingMode.SRPBatcher)
                    {
                        passPre += "                #define VLB_SRP_BATCHER 1" + System.Environment.NewLine;

                        if (rp == SRPHelper.RenderPipeline.URP)
                        {
                            // force enable constant buffers to fix SRP Batcher support on Android
                            passPre += "                #pragma enable_cbuffer" + System.Environment.NewLine;
                        }
                    }
                }

                if (ditheringEnabled)
                {
                    passPre += "                #define VLB_DITHERING 1" + System.Environment.NewLine;
                }

                passPre += LoadText(Instance.GetTextAssetIncludes(rp));

                code = code.Replace("{VLB_GEN_PRE}", passPre);

                return code;
            }
        }

        class GenShader
        {
            SRPHelper.RenderPipeline m_RenderPipeline;
            RenderingMode m_RenderingMode;
            bool m_DitheringEnabled;
            List<GenPass> m_Passes = new List<GenPass>();

            public GenShader(RenderPipeline rp, RenderingMode rm, bool ditheringEnabled)
            {
                m_RenderingMode = rm;
                m_DitheringEnabled = ditheringEnabled;

                switch (rp)
                {
                    case RenderPipeline.BuiltIn:
                        AddPass(CullMode.Front);
                        if (rm == RenderingMode.MultiPass) AddPass(CullMode.Back);
                        m_RenderPipeline = SRPHelper.RenderPipeline.BuiltIn;
                        break;

                    case RenderPipeline.URP:
                        AddPass(CullMode.Front);
                        m_RenderPipeline = SRPHelper.RenderPipeline.URP;
                        break;

                    case RenderPipeline.HDRP:
                        AddPass(CullMode.Front);
                        m_RenderPipeline = SRPHelper.RenderPipeline.HDRP;
                        break;
                }
            }

            GenShader AddPass(CullMode cullMode)
            {
                m_Passes.Add(new GenPass(cullMode));
                return this;
            }

            public Shader Generate()
            {
                var shaderName = string.Format("Hidden/VLB_{0}_{1}", m_RenderPipeline, m_RenderingMode);
                var code = LoadText(Instance.textAssetBase);
                code = code.Replace("{VLB_GEN_SHADERNAME}", shaderName);

                var passes = "";
                for (int i = 0; i < m_Passes.Count; ++i)
                    passes += m_Passes[i].Generate(m_RenderPipeline, m_RenderingMode, m_DitheringEnabled, i, m_Passes.Count);

                code = code.Replace("{VLB_GEN_PASSES}", passes);
                code = code.Replace("{VLB_GEN_SPECIFIC_INCLUDE}", GetRenderPipelineInclude(m_RenderPipeline));

                var outputFolderPath = ShaderGenerator.GetOutputPath();
                var outputFilePath = Path.Combine(outputFolderPath, kShaderAssetName);
                var outputFullPath = System.IO.Path.Combine(Application.dataPath, outputFilePath + ".shader");
                File.WriteAllText(outputFullPath, code);
                AssetDatabase.Refresh();

                var shader = Shader.Find(shaderName);
                Debug.Assert(shader != null, string.Format("Failed to generate shader '{0}' at '{1}'", shaderName, outputFullPath));
                return shader;
            }

            string GetRenderPipelineInclude(SRPHelper.RenderPipeline rp)
            {
                switch(rp)
                {
                    case SRPHelper.RenderPipeline.BuiltIn: return "ShaderSpecificBuiltin.cginc";
                    case SRPHelper.RenderPipeline.HDRP: return "ShaderSpecificHDRP.hlsl";
                    case SRPHelper.RenderPipeline.LWRP:
                    case SRPHelper.RenderPipeline.URP: return "ShaderSpecificURP.cginc";
                }
                return null;
            }
        }

        static ShaderGenerator FindInstance()
        {
            var assetGUIDs = AssetDatabase.FindAssets("ShaderGenerator");

            foreach (var guid in assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ShaderGenerator>(path);
                if (asset)
                    return asset;
            }
            return null;
        }

        // Store data to generate the shader on OnEnable
        class GenerationParam
        {
            public RenderPipeline renderPipeline;
            public RenderingMode renderingMode;
            public bool ditheringEnabled;
        }

        static GenerationParam ms_GenerationParamOnEnable = null;

        void OnEnable()
        {
            if(ms_GenerationParamOnEnable != null && Instance != null)
            {
                Generate(ms_GenerationParamOnEnable.renderPipeline, ms_GenerationParamOnEnable.renderingMode, ms_GenerationParamOnEnable.ditheringEnabled);
                ms_GenerationParamOnEnable = null;
            }
        }

        // Singleton management
        static ShaderGenerator m_Instance = null;
        static ShaderGenerator Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = FindInstance();
                return m_Instance;
            }
        }
    }
}
#endif


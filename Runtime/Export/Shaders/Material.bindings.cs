// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using UnityEngine.Bindings;
using UnityEngine.Scripting;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

namespace UnityEngine
{
    [NativeHeader("Runtime/Graphics/ShaderScriptBindings.h")]
    [NativeHeader("Runtime/Shaders/Material.h")]
    public partial class Material : Object
    {
        [FreeFunction("MaterialScripting::CreateWithShader")]   extern private static void CreateWithShader([Writable] Material self, [NotNull] Shader shader);
        [FreeFunction("MaterialScripting::CreateWithMaterial")] extern private static void CreateWithMaterial([Writable] Material self, [NotNull] Material source);
        [FreeFunction("MaterialScripting::CreateWithString")]   extern private static void CreateWithString([Writable] Material self);

        public Material(Shader shader)   { CreateWithShader(this, shader); }
        // will otherwise be stripped if scene only uses default materials not explicitly referenced
        // (ie some components will get a default material if a material reference is null)
        [RequiredByNativeCode]
        public Material(Material source) { CreateWithMaterial(this, source); }

        // TODO: is it time to make it deprecated with error?
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Creating materials from shader source string is no longer supported. Use Shader assets instead.", false)]
        public Material(string contents) { CreateWithString(this); }

        static extern internal Material GetDefaultMaterial();
        static extern internal Material GetDefaultParticleMaterial();
        static extern internal Material GetDefaultLineMaterial();

        extern public Shader shader { get; set; }

        public Color color
        {
            get
            {
                // Try to find property with [MainColor] attribute and use that, otherwise fallback to old hardcoded one.
                int nameId = GetFirstPropertyNameIdByAttribute(ShaderPropertyFlags.MainColor);
                if (nameId >= 0)
                    return GetColor(nameId);
                else
                    return GetColor("_Color");
            }
            set
            {
                int nameId = GetFirstPropertyNameIdByAttribute(ShaderPropertyFlags.MainColor);
                if (nameId >= 0)
                    SetColor(nameId, value);
                else
                    SetColor("_Color", value);
            }
        }
        public Texture mainTexture
        {
            get
            {
                // Try to find property with [MainTexture] attribute and use that, otherwise fallback to old hardcoded one.
                int nameId = GetFirstPropertyNameIdByAttribute(ShaderPropertyFlags.MainTexture);
                if (nameId >= 0)
                    return GetTexture(nameId);
                else
                    return GetTexture("_MainTex");
            }
            set
            {
                int nameId = GetFirstPropertyNameIdByAttribute(ShaderPropertyFlags.MainTexture);
                if (nameId >= 0)
                    SetTexture(nameId, value);
                else
                    SetTexture("_MainTex", value);
            }
        }
        public Vector2 mainTextureOffset
        {
            get
            {
                int nameId = GetFirstPropertyNameIdByAttribute(ShaderPropertyFlags.MainTexture);
                if (nameId >= 0)
                    return GetTextureOffset(nameId);
                else
                    return GetTextureOffset("_MainTex");
            }
            set
            {
                int nameId = GetFirstPropertyNameIdByAttribute(ShaderPropertyFlags.MainTexture);
                if (nameId >= 0)
                    SetTextureOffset(nameId, value);
                else
                    SetTextureOffset("_MainTex", value);
            }
        }
        public Vector2 mainTextureScale
        {
            get
            {
                int nameId = GetFirstPropertyNameIdByAttribute(ShaderPropertyFlags.MainTexture);
                if (nameId >= 0)
                    return GetTextureScale(nameId);
                else
                    return GetTextureScale("_MainTex");
            }
            set
            {
                int nameId = GetFirstPropertyNameIdByAttribute(ShaderPropertyFlags.MainTexture);
                if (nameId >= 0)
                    SetTextureScale(nameId, value);
                else
                    SetTextureScale("_MainTex", value);
            }
        }
        [NativeName("GetFirstPropertyNameIdByAttributeFromScript")] extern private int GetFirstPropertyNameIdByAttribute(ShaderPropertyFlags attributeFlag);

        [NativeName("HasPropertyFromScript")] extern public bool HasProperty(int nameID);
        public bool HasProperty(string name) { return HasProperty(Shader.PropertyToID(name)); }

        extern public int renderQueue {[NativeName("GetActualRenderQueue")] get; [NativeName("SetCustomRenderQueue")] set; }
        extern internal int rawRenderQueue {[NativeName("GetCustomRenderQueue")] get; }

        extern public void EnableKeyword(string keyword);
        extern public void DisableKeyword(string keyword);
        extern public bool IsKeywordEnabled(string keyword);

        extern public MaterialGlobalIlluminationFlags globalIlluminationFlags { get; set; }
        extern public bool doubleSidedGI { get; set; }
        [NativeProperty("EnableInstancingVariants")] extern public bool enableInstancing { get; set; }

        extern public int passCount { [NativeName("GetShader()->GetPassCount")] get; }
        [FreeFunction("MaterialScripting::SetShaderPassEnabled", HasExplicitThis = true)] extern public void SetShaderPassEnabled(string passName, bool enabled);
        [FreeFunction("MaterialScripting::GetShaderPassEnabled", HasExplicitThis = true)] extern public bool GetShaderPassEnabled(string passName);
        extern public string GetPassName(int pass);
        extern public int FindPass(string passName);

        extern public void SetOverrideTag(string tag, string val);
        [NativeName("GetTag")] extern private string GetTagImpl(string tag, bool currentSubShaderOnly, string defaultValue);
        public string GetTag(string tag, bool searchFallbacks, string defaultValue) { return GetTagImpl(tag, !searchFallbacks, defaultValue); }
        public string GetTag(string tag, bool searchFallbacks) { return GetTagImpl(tag, !searchFallbacks, ""); }

        [NativeThrows]
        [FreeFunction("MaterialScripting::Lerp", HasExplicitThis = true)] extern public void Lerp(Material start, Material end, float t);
        [FreeFunction("MaterialScripting::SetPass", HasExplicitThis = true)] extern public bool SetPass(int pass);
        [FreeFunction("MaterialScripting::CopyPropertiesFrom", HasExplicitThis = true)] extern public void CopyPropertiesFromMaterial(Material mat);

        [FreeFunction("MaterialScripting::GetShaderKeywords", HasExplicitThis = true)] extern private string[] GetShaderKeywords();
        [FreeFunction("MaterialScripting::SetShaderKeywords", HasExplicitThis = true)] extern private void SetShaderKeywords(string[] names);
        public string[] shaderKeywords { get { return GetShaderKeywords(); } set { SetShaderKeywords(value); } }

        extern public int ComputeCRC();

        [FreeFunction("MaterialScripting::GetTexturePropertyNames", HasExplicitThis = true)]
        extern public String[] GetTexturePropertyNames();

        [FreeFunction("MaterialScripting::GetTexturePropertyNameIDs", HasExplicitThis = true)]
        extern public int[] GetTexturePropertyNameIDs();

        [FreeFunction("MaterialScripting::GetTexturePropertyNamesInternal", HasExplicitThis = true)]
        extern private void GetTexturePropertyNamesInternal(object outNames);

        [FreeFunction("MaterialScripting::GetTexturePropertyNameIDsInternal", HasExplicitThis = true)]
        extern private void GetTexturePropertyNameIDsInternal(object outNames);

        public void GetTexturePropertyNames(List<string> outNames)
        {
            if (outNames == null)
            {
                throw new ArgumentNullException(nameof(outNames));
            }

            GetTexturePropertyNamesInternal(outNames);
        }

        public void GetTexturePropertyNameIDs(List<int> outNames)
        {
            if (outNames == null)
            {
                throw new ArgumentNullException(nameof(outNames));
            }

            GetTexturePropertyNameIDsInternal(outNames);
        }


        // TODO: get buffer is missing

        [NativeName("SetFloatFromScript")]   extern private void SetFloatImpl(int name, float value);
        [NativeName("SetColorFromScript")]   extern private void SetColorImpl(int name, Color value);
        [NativeName("SetMatrixFromScript")]  extern private void SetMatrixImpl(int name, Matrix4x4 value);
        [NativeName("SetTextureFromScript")] extern private void SetTextureImpl(int name, Texture value);
        [NativeName("SetRenderTextureFromScript")] extern private void SetRenderTextureImpl(int name, RenderTexture value, Rendering.RenderTextureSubElement element);
        [NativeName("SetBufferFromScript")]  extern private void SetBufferImpl(int name, ComputeBuffer value);
        [NativeName("SetGraphicsBufferFromScript")]  extern private void SetGraphicsBufferImpl(int name, GraphicsBuffer value);
        [NativeName("SetConstantBufferFromScript")] extern private void SetConstantBufferImpl(int name, ComputeBuffer value, int offset, int size);
        [NativeName("SetConstantGraphicsBufferFromScript")] extern private void SetConstantGraphicsBufferImpl(int name, GraphicsBuffer value, int offset, int size);

        [NativeName("GetFloatFromScript")]   extern private float     GetFloatImpl(int name);
        [NativeName("GetColorFromScript")]   extern private Color     GetColorImpl(int name);
        [NativeName("GetMatrixFromScript")]  extern private Matrix4x4 GetMatrixImpl(int name);
        [NativeName("GetTextureFromScript")] extern private Texture   GetTextureImpl(int name);

        [FreeFunction(Name = "MaterialScripting::SetFloatArray", HasExplicitThis = true)]  extern private void SetFloatArrayImpl(int name, float[] values, int count);
        [FreeFunction(Name = "MaterialScripting::SetVectorArray", HasExplicitThis = true)] extern private void SetVectorArrayImpl(int name, Vector4[] values, int count);
        [FreeFunction(Name = "MaterialScripting::SetColorArray", HasExplicitThis = true)]  extern private void SetColorArrayImpl(int name, Color[] values, int count);
        [FreeFunction(Name = "MaterialScripting::SetMatrixArray", HasExplicitThis = true)] extern private void SetMatrixArrayImpl(int name, Matrix4x4[] values, int count);

        [FreeFunction(Name = "MaterialScripting::GetFloatArray", HasExplicitThis = true)]  extern private float[]     GetFloatArrayImpl(int name);
        [FreeFunction(Name = "MaterialScripting::GetVectorArray", HasExplicitThis = true)] extern private Vector4[]   GetVectorArrayImpl(int name);
        [FreeFunction(Name = "MaterialScripting::GetColorArray", HasExplicitThis = true)]  extern private Color[]     GetColorArrayImpl(int name);
        [FreeFunction(Name = "MaterialScripting::GetMatrixArray", HasExplicitThis = true)] extern private Matrix4x4[] GetMatrixArrayImpl(int name);

        [FreeFunction(Name = "MaterialScripting::GetFloatArrayCount", HasExplicitThis = true)]  extern private int GetFloatArrayCountImpl(int name);
        [FreeFunction(Name = "MaterialScripting::GetVectorArrayCount", HasExplicitThis = true)] extern private int GetVectorArrayCountImpl(int name);
        [FreeFunction(Name = "MaterialScripting::GetColorArrayCount", HasExplicitThis = true)]  extern private int GetColorArrayCountImpl(int name);
        [FreeFunction(Name = "MaterialScripting::GetMatrixArrayCount", HasExplicitThis = true)] extern private int GetMatrixArrayCountImpl(int name);

        [FreeFunction(Name = "MaterialScripting::ExtractFloatArray", HasExplicitThis = true)]  extern private void ExtractFloatArrayImpl(int name, [Out] float[] val);
        [FreeFunction(Name = "MaterialScripting::ExtractVectorArray", HasExplicitThis = true)] extern private void ExtractVectorArrayImpl(int name, [Out] Vector4[] val);
        [FreeFunction(Name = "MaterialScripting::ExtractColorArray", HasExplicitThis = true)]  extern private void ExtractColorArrayImpl(int name, [Out] Color[] val);
        [FreeFunction(Name = "MaterialScripting::ExtractMatrixArray", HasExplicitThis = true)] extern private void ExtractMatrixArrayImpl(int name, [Out] Matrix4x4[] val);

        [NativeName("GetTextureScaleAndOffsetFromScript")] extern private Vector4 GetTextureScaleAndOffsetImpl(int name);
        [NativeName("SetTextureOffsetFromScript")] extern private void SetTextureOffsetImpl(int name, Vector2 offset);
        [NativeName("SetTextureScaleFromScript")]  extern private void SetTextureScaleImpl(int name, Vector2 scale);
    }
}
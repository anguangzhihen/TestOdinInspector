#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TextureUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Collection of texture functions.
    /// </summary>
    public static class TextureUtilities
    {
        private static Material extractSpriteMaterial;

        private static readonly string extractSpriteShader = @"
            Shader ""Hidden/Sirenix/Editor/GUIIcon""
            {
	            Properties
	            {
                    _MainTex(""Texture"", 2D) = ""white"" {}
                    _Color(""Color"", Color) = (1,1,1,1)
                    _Rect(""Rect"", Vector) = (0,0,0,0)
                    _TexelSize(""TexelSize"", Vector) = (0,0,0,0)
	            }
                SubShader
	            {
                    Blend SrcAlpha OneMinusSrcAlpha
                    Pass
                    {
                        CGPROGRAM
                            " + "#" + @"pragma vertex vert
                            " + "#" + @"pragma fragment frag
                            " + "#" + @"include ""UnityCG.cginc""

                            struct appdata
                            {
                                float4 vertex : POSITION;
					            float2 uv : TEXCOORD0;
				            };

                            struct v2f
                            {
                                float2 uv : TEXCOORD0;
					            float4 vertex : SV_POSITION;
				            };

                            sampler2D _MainTex;
                            float4 _Rect;

                            v2f vert(appdata v)
                            {
                                v2f o;
                                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                                o.uv = v.uv;
                                return o;
                            }

                            fixed4 frag(v2f i) : SV_Target
				            {
                                float2 uv = i.uv;
                                uv *= _Rect.zw;
					            uv += _Rect.xy;
					            return tex2D(_MainTex, uv);
				            }
			            ENDCG
		            }
	            }
            }";


        private static Func<int, int, byte[], Texture2D> tryLoadImage;

        /// <summary>
        /// Loads an image from bytes with the specified width and height. Use this instead of someTexture.LoadImage() if you're compiling to an assembly. Unity has moved the method in 2017, 
        /// and Unity's assembly updater is not able to fix it for you. This searches for a proper LoadImage method in multiple locations, and also handles type name conflicts.
        /// </summary>
        public static Texture2D LoadImage(int width, int height, byte[] bytes)
        {
            if (tryLoadImage == null)
            {
                System.Reflection.MethodInfo loadImageMethodInfo = typeof(Texture2D).GetMethod("LoadImage", new Type[] { typeof(byte[]) });

                if (loadImageMethodInfo == null)
                {
                    loadImageMethodInfo = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(x => x.FullName.Contains("UnityEngine"))
                        .Select(x => x.GetType("UnityEngine.ImageConversion"))
                        .Where(x => x != null)
                        .Select(x =>
                        {
                            return x.GetMethod("LoadImage", new Type[] { typeof(Texture2D), typeof(byte[]) }) ??
                                   x.GetMethod("LoadImage", new Type[] { typeof(Texture2D), typeof(byte[]), typeof(bool) });
                        })
                        .Where(x => x != null)
                        .FirstOrDefault();
                }

                if (loadImageMethodInfo == null)
                {
                    Debug.LogError("No LoadMethod was found in either UnityEngine.Texture2D or UnityEngine.ImageConversion. All Odin editor icons will be broken.");
                    tryLoadImage = (w, h, b) =>
                    {
                        var t = new Texture2D(1, 1);
                        t.SetPixel(0, 0, new Color(1, 0, 1));
                        t.Apply();
                        return t;
                    };
                }
                else
                {
                    if (loadImageMethodInfo.IsStatic())
                    {
                        if (loadImageMethodInfo.GetParameters().Count() == 3)
                        {
                            tryLoadImage = (w, h, b) =>
                            {
                                var tex = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
                                loadImageMethodInfo.Invoke(null, new object[] { tex, b, false });
                                return tex;
                            };
                        }
                        else
                        {
                            tryLoadImage = (w, h, b) =>
                            {
                                var tex = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
                                loadImageMethodInfo.Invoke(null, new object[] { tex, b });
                                return tex;
                            };
                        }
                    }
                    else
                    {
                        tryLoadImage = (w, h, b) =>
                        {
                            var tex = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
                            loadImageMethodInfo.Invoke(tex, new object[] { b });
                            return tex;
                        };
                    }
                }

            }

            return tryLoadImage(width, height, bytes);
        }


        /// <summary>
        /// Crops a Texture2D into a new Texture2D.
        /// </summary>
        public static Texture2D CropTexture(this Texture texture, Rect source)
        {
            RenderTexture prev = RenderTexture.active;
            var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 8);
            RenderTexture.active = rt;
            GL.Clear(false, true, new Color(1, 1, 1, 0));
            Graphics.Blit(texture, rt);

            Texture2D clone = new Texture2D((int)source.width, (int)source.height, TextureFormat.ARGB32, false, true);
            clone.filterMode = FilterMode.Point;
            clone.ReadPixels(source, 0, 0);
            clone.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return clone;
        }

        /// <summary>
        /// Converts a Sprite to a Texture2D.
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public static Texture2D ConvertSpriteToTexture(Sprite sprite)
        {
            var rect = sprite.rect;

            if (extractSpriteMaterial == null || extractSpriteMaterial.shader == null)
            {
                extractSpriteMaterial = new Material(ShaderUtil.CreateShaderAsset(extractSpriteShader));
            }

            extractSpriteMaterial.SetVector("_TexelSize", new Vector2(1f / sprite.texture.width, 1f / sprite.texture.height));
            extractSpriteMaterial.SetVector("_Rect", new Vector4(
                rect.x / sprite.texture.width,
                rect.y / sprite.texture.height,
                rect.width / sprite.texture.width,
                rect.height / sprite.texture.height
            ));

            var prevSRGB = GL.sRGBWrite;
            GL.sRGBWrite = true;
            RenderTexture prev = RenderTexture.active;
            var rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height, 0);
            RenderTexture.active = rt;
            GL.Clear(false, true, new Color(1, 1, 1, 0));
            Graphics.Blit(sprite.texture, rt, extractSpriteMaterial);


            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false, true);
            texture.filterMode = FilterMode.Bilinear;
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.alphaIsTransparency = true;
            texture.Apply();

            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = prev;
            GL.sRGBWrite = prevSRGB;
            return texture;
        }
    }
}
#endif
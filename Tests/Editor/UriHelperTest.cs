﻿// Copyright 2020-2021 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.IO;
using NUnit.Framework;
using UnityEngine.Profiling;

namespace GLTFast.Tests
{
    public class UriHelperTest
    {
        static Uri[] glb = new []{
            new Uri("file.glb",UriKind.RelativeOrAbsolute),
            new Uri("file:///dir/sub/file.glb",UriKind.RelativeOrAbsolute),
            new Uri("http://www.server.com/dir/sub/file.glb",UriKind.RelativeOrAbsolute),
            new Uri("http://www.server.com/dir/sub/file.glb",UriKind.RelativeOrAbsolute),
            new Uri("http://www.server.com/dir/sub/file.glb?a=123&b=234",UriKind.RelativeOrAbsolute),
        };
        static Uri[] gltf = new []{
            new Uri("file.gltf",UriKind.RelativeOrAbsolute),
            new Uri("file:///dir/sub/file.gltf",UriKind.RelativeOrAbsolute),
            new Uri("http://www.server.com/dir/sub/file.gltf",UriKind.RelativeOrAbsolute),
            new Uri("http://www.server.com/dir/sub/file.gltf",UriKind.RelativeOrAbsolute),
            new Uri("http://www.server.com/dir/sub/file.gltf?a=123&b=234",UriKind.RelativeOrAbsolute),
        };
        static Uri[] unknown = new []{
            new Uri("f",UriKind.RelativeOrAbsolute),
            new Uri("file:///dir/sub/f",UriKind.RelativeOrAbsolute),
            new Uri("http://www.server.com/dir/sub/f",UriKind.RelativeOrAbsolute),
            new Uri("http://www.server.com/dir/sub/f",UriKind.RelativeOrAbsolute),
            new Uri("http://www.server.com/dir/sub/f?a=123&b=234)",UriKind.RelativeOrAbsolute),
        };

        [Test]
        public void GetBaseUriTest()
        {
            // HTTP(s) gltf
            Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("http://www.server.com/dir/sub/file.gltf")));
            Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("https://www.server.com/dir/sub/file.gltf")));
            Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("http://www.server.com/dir/sub/file.gltf?a=123&b=456")));
            Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("https://www.server.com/dir/sub/file.gltf?a=123&b=456")));
            // HTTP(s) glb
            Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("http://www.server.com/dir/sub/file.glb")));
            Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("https://www.server.com/dir/sub/file.glb")));
            Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("http://www.server.com/dir/sub/file.glb?a=123&b=456")));
            Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("https://www.server.com/dir/sub/file.glb?a=123&b=456")));
            // HTTP(s) none
            Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("http://www.server.com/dir/sub/file")));
            Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("https://www.server.com/dir/sub/file")));
            Assert.AreEqual(new Uri("http://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("http://www.server.com/dir/sub/file?a=123&b=456")));
            Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("https://www.server.com/dir/sub/file?a=123&b=456")));

            // file paths
            Assert.AreEqual(new Uri("file:///dir/sub/"),UriHelper.GetBaseUri(new Uri("file:///dir/sub/file.gltf")));
            Assert.AreEqual(new Uri("file://c:\\dir\\sub\\"),UriHelper.GetBaseUri(new Uri("c:\\dir\\sub\\file.gltf")));
#if !UNITY_EDITOR_WIN
            Assert.AreEqual(new Uri("file:///dir/sub/"),UriHelper.GetBaseUri(new Uri("/dir/sub/file.gltf")));
#endif

            // special char `+`
            Assert.AreEqual(new Uri("https://www.server.com/dir/sub/"),UriHelper.GetBaseUri(new Uri("https://www.server.com/dir/sub/file+test.gltf")));
            Assert.AreEqual(new Uri("file:///dir/sub/"),UriHelper.GetBaseUri(new Uri("file:///dir/sub/file+test.gltf")));
            
            
            // relative paths
            var uri = new Uri("Assets/Some/Path/asset.glb", UriKind.Relative);
            Assert.AreEqual(new Uri("Assets/Some/Path",UriKind.Relative),UriHelper.GetBaseUri(uri));
        }

        [Test]
        public void GetUriStringTest()
        {
            var baseUri = new Uri("http://www.server.com/dir/sub/");
            
            Assert.AreEqual(
                "file+test.gltf",
                UriHelper.GetUriString("file+test.gltf",null).ToString()
                );
            Assert.AreEqual(
                "http://www.server.com/dir/sub/file+test.gltf",
                UriHelper.GetUriString("file+test.gltf",baseUri).ToString()
                );
            Assert.AreEqual(
                "http://www.server.com/dir/sub/sub2/sub3/file+test.gltf",
                UriHelper.GetUriString("sub2/sub3/file+test.gltf",baseUri).ToString()
                );
            Assert.AreEqual(
                "http://www.server.com/dir/sub/file.gltf",
                UriHelper.GetUriString("./file.gltf",baseUri).ToString()
                );
            Assert.AreEqual(
                "http://www.server.com/dir/file.gltf",
                UriHelper.GetUriString("../file.gltf",baseUri).ToString()
                );
            Assert.AreEqual(
                "http://www.server.com/dir/x/file.gltf",
                UriHelper.GetUriString("../x/file.gltf",baseUri).ToString()
                );
            Assert.AreEqual(
                "http://www.server.com/dir/other_folder/texture.png",
                UriHelper.GetUriString("../other_folder/texture.png",baseUri).ToString()
                );
            Assert.AreEqual(
                "http://www.server.com/dir/sub/asset.glb",
                UriHelper.GetUriString("asset.glb",baseUri).ToString()
                );
            Assert.AreEqual(
                "http://www.server.com/other_folder/texture.png",
                UriHelper.GetUriString("../../../../other_folder/texture.png",baseUri).ToString()
            );
            Assert.AreEqual("http://www.server.com/dir/sub/", baseUri.ToString());
            
            var relBaseUri = new Uri("Assets/Some/Path", UriKind.Relative);
            Assert.AreEqual(
                "Assets/Some/Path/asset.glb",
                UriHelper.GetUriString("asset.glb",relBaseUri).ToString()
                );
            Assert.AreEqual(
                "Assets/Some/other_folder/texture.png",
                UriHelper.GetUriString("../other_folder/texture.png",relBaseUri).ToString()
                );
            Assert.AreEqual(
                "other_folder/texture.png",
                UriHelper.GetUriString("../../../other_folder/texture.png",relBaseUri).ToString()
                );
            Assert.AreEqual(
                "other_folder/texture.png",
                UriHelper.GetUriString("../../../../other_folder/texture.png",relBaseUri).ToString()
                );
            Assert.AreEqual(
                new Uri("Assets/Some/Path", UriKind.Relative),
                relBaseUri
                );
        }

        [Test]
        public void RemoveDotSegments() {
            var s = UriHelper.RemoveDotSegments("file.txt", out var parentLevels);
            Assert.AreEqual("file.txt",s);
            Assert.AreEqual(0,parentLevels);
            
            s = UriHelper.RemoveDotSegments("../other_folder/file.txt", out parentLevels);
            Assert.AreEqual("other_folder/file.txt",s);
            Assert.AreEqual(1,parentLevels);
            
            s = UriHelper.RemoveDotSegments("other_folder/../file.txt", out parentLevels);
            Assert.AreEqual("file.txt",s);
            Assert.AreEqual(0,parentLevels);
            
            s = UriHelper.RemoveDotSegments("other_folder/./file.txt", out parentLevels);
            Assert.AreEqual("other_folder/file.txt",s);
            Assert.AreEqual(0,parentLevels);
            
            s = UriHelper.RemoveDotSegments("other_folder/./../x/../file.txt", out parentLevels);
            Assert.AreEqual("file.txt",s);
            Assert.AreEqual(0,parentLevels);
            
            s = UriHelper.RemoveDotSegments("../other_folder/../x/../file.txt", out parentLevels);
            Assert.AreEqual("file.txt",s);
            Assert.AreEqual(1,parentLevels);
        }

        [Test]
        public void IsGltfBinaryTest()
        {
            Profiler.BeginSample("IsGltfBinaryProfile");
            for (int i = 0; i < 1000; i++)
            {
                for (int j = 0; j < glb.Length; j++) {
                    Profiler.BeginSample("IsGltfBinaryUriTrue");
                    Assert.IsTrue(UriHelper.IsGltfBinary(glb[j]));
                    Profiler.EndSample();
                }
                for (int j = 0; j < gltf.Length; j++) {
                    Profiler.BeginSample("IsGltfBinaryUriFalse");
                    Assert.IsFalse(UriHelper.IsGltfBinary(gltf[j]));
                    Profiler.EndSample();
                }
                for (int j = 0; j < unknown.Length; j++) {
                    Profiler.BeginSample("IsGltfBinaryUriUnknown");
                    Assert.IsNull(UriHelper.IsGltfBinary(unknown[j]));
                    Profiler.EndSample();
                }
            }
            Profiler.EndSample();
        }

        [Test]
        public void GetImageFormatFromUriTest() {
            Assert.AreEqual(ImageFormat.Unknown, UriHelper.GetImageFormatFromUri(null)); // shortest path
            Assert.AreEqual(ImageFormat.Unknown, UriHelper.GetImageFormatFromUri("")); // shortest path
            Assert.AreEqual(ImageFormat.Unknown, UriHelper.GetImageFormatFromUri("f")); // shortest path
            
            Assert.AreEqual(ImageFormat.Jpeg, UriHelper.GetImageFormatFromUri("f.jpg")); // shortest path
            Assert.AreEqual(ImageFormat.Jpeg, UriHelper.GetImageFormatFromUri("file:///Some/Path/file.jpg"));
            Assert.AreEqual(ImageFormat.Jpeg, UriHelper.GetImageFormatFromUri("http://server.com/some.Path/file.jpg"));
            Assert.AreEqual(ImageFormat.Jpeg, UriHelper.GetImageFormatFromUri("https://server.com/some.Path/file.jpg?key=value.with.dots&otherkey=val&arrval[]=x"));
            
            Assert.AreEqual(ImageFormat.Jpeg, UriHelper.GetImageFormatFromUri("f.jpeg")); // shortest path
            Assert.AreEqual(ImageFormat.Jpeg, UriHelper.GetImageFormatFromUri("file:///Some/Path/file.jpeg"));
            Assert.AreEqual(ImageFormat.Jpeg, UriHelper.GetImageFormatFromUri("http://server.com/some.Path/file.jpeg"));
            Assert.AreEqual(ImageFormat.Jpeg, UriHelper.GetImageFormatFromUri("https://server.com/some.Path/file.jpeg?key=value.with.dots&otherkey=val&arrval[]=x"));
            
            Assert.AreEqual(ImageFormat.PNG, UriHelper.GetImageFormatFromUri("f.png")); // shortest path
            Assert.AreEqual(ImageFormat.PNG, UriHelper.GetImageFormatFromUri("file:///Some/Path/file.png"));
            Assert.AreEqual(ImageFormat.PNG, UriHelper.GetImageFormatFromUri("http://server.com/some.Path/file.png"));
            Assert.AreEqual(ImageFormat.PNG, UriHelper.GetImageFormatFromUri("https://server.com/some.Path/file.png?key=value.with.dots&otherkey=val&arrval[]=x"));
            
            Assert.AreEqual(ImageFormat.KTX, UriHelper.GetImageFormatFromUri("f.ktx")); // shortest path
            Assert.AreEqual(ImageFormat.KTX, UriHelper.GetImageFormatFromUri("file:///Some/Path/file.ktx"));
            Assert.AreEqual(ImageFormat.KTX, UriHelper.GetImageFormatFromUri("http://server.com/some.Path/file.ktx"));
            Assert.AreEqual(ImageFormat.KTX, UriHelper.GetImageFormatFromUri("https://server.com/some.Path/file.ktx?key=value.with.dots&otherkey=val&arrval[]=x"));
            
            Assert.AreEqual(ImageFormat.KTX, UriHelper.GetImageFormatFromUri("f.ktx2")); // shortest path
            Assert.AreEqual(ImageFormat.KTX, UriHelper.GetImageFormatFromUri("file:///Some/Path/file.ktx2"));
            Assert.AreEqual(ImageFormat.KTX, UriHelper.GetImageFormatFromUri("http://server.com/some.Path/file.ktx2"));
            Assert.AreEqual(ImageFormat.KTX, UriHelper.GetImageFormatFromUri("https://server.com/some.Path/file.ktx2?key=value.with.dots&otherkey=val&arrval[]=x"));
        }
    }
}
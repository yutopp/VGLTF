﻿//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VGltf.Unity
{
    public abstract class MaterialImporterHook
    {
        public abstract IndexedResource<Material> Import(MaterialImporter importer, int matIndex);
    }

    public class MaterialImporter : ImporterRefHookable<MaterialImporterHook>
    {
        public override IImporterContext Context { get; }

        public MaterialImporter(IImporterContext context)
        {
            Context = context;
        }

        public IndexedResource<Material> Import(int matIndex)
        {
            var gltf = Context.Container.Gltf;
            var gltfMat = gltf.Materials[matIndex];

            return Context.RuntimeResources.Materials.GetOrCall(matIndex, () => {
                return ForceImport(matIndex);
            });
        }

        public IndexedResource<Material> ForceImport(int matIndex)
        {
            foreach(var h in Hooks)
            {
                var r = h.Import(this, matIndex);
                if (r != null)
                {
                    return r;
                }
            }

            // Default import
            var gltf = Context.Container.Gltf;
            var gltfMat = gltf.Materials[matIndex];

            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                throw new NotImplementedException();
            }

            var mat = new Material(shader);
            mat.name = gltfMat.Name;

            var resource = Context.RuntimeResources.Materials.Add(matIndex, matIndex, mat);

            if (gltfMat.PbrMetallicRoughness != null)
            {
                var pbrMR = gltfMat.PbrMetallicRoughness;
                if (pbrMR.BaseColorTexture != null)
                {
                    var bct = pbrMR.BaseColorTexture;
                    var textureResource = Context.Textures.Import(bct.Index);
                    mat.SetTexture("_MainTex", textureResource.Value);
                }
            }

            return resource;
        }
    }
}

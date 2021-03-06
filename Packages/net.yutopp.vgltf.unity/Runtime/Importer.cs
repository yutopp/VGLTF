//
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
    public abstract class ImporterHookBase
    {
        public virtual void PostHook(Importer importer, Transform parentTrans)
        {
        }
    }

    public class Importer : ImporterRefHookable<ImporterHookBase>, IDisposable
    {
        class InnerContext : IImporterContext
        {
            public GltfContainer Container { get; }
            public ImporterRuntimeResources RuntimeResources { get; }
            public ResourcesStore BufferView { get; }

            public NodeImporter Nodes { get; }
            public MeshImporter Meshes { get; }
            public MaterialImporter Materials { get; }
            public TextureImporter Textures { get; }
            public ImageImporter Images { get; }

            public InnerContext(GltfContainer container, IResourceLoader loader)
            {
                Container = container;
                RuntimeResources = new ImporterRuntimeResources();
                BufferView = new ResourcesStore(container.Gltf, container.Buffer, loader);

                Nodes = new NodeImporter(this);
                Meshes = new MeshImporter(this);
                Materials = new MaterialImporter(this);
                Textures = new TextureImporter(this);
                Images = new ImageImporter(this);
            }
        }

        public override IImporterContext Context { get; }

        bool _disposed = false;

        public Importer(GltfContainer container, IResourceLoader loader)
        {
            Context = new InnerContext(container, loader);
        }

        public Importer(GltfContainer container)
            : this(container, new ResourceLoaderFromEmbedOnly())
        {
        }

        public void ImportSceneNodes(GameObject parentGo)
        {
            var gltf = Context.Container.Gltf;
            if (gltf.Scene == null)
            {
                throw new Exception("Scene is null");
            }

            var gltfScene = gltf.Scenes[gltf.Scene.Value];

            var nodesCache = new NodesCache();
            foreach (var nodeIndex in gltfScene.Nodes)
            {
                Context.Nodes.ImportGameObjects(nodeIndex, nodesCache, parentGo);
            }
            foreach (var nodeIndex in gltfScene.Nodes)
            {
                Context.Nodes.ImportMeshesAndSkins(nodeIndex, nodesCache);
            }

            foreach(var hook in Hooks)
            {
                hook.PostHook(this, parentGo.transform);
            }

            return;
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: remove resources;
            }

            _disposed = true;
        }
    }
}

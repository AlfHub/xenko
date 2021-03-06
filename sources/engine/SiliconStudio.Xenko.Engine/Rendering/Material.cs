// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A compiled version of <see cref="MaterialDescriptor"/>.
    /// </summary>
    [ReferenceSerializer, DataSerializerGlobal(typeof(ReferenceSerializer<Material>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializer<Material>))]
    [DataContract]
    public class Material
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class.
        /// </summary>
        public Material()
        {
            Passes = new MaterialPassCollection(this);
        }

        /// <summary>
        /// The passes contained in this material (usually one).
        /// </summary>
        public MaterialPassCollection Passes { get; }

        /// <summary>
        /// Gets or sets the descriptor (this field is null at runtime).
        /// </summary>
        /// <value>The descriptor.</value>
        [DataMemberIgnore]
        public MaterialDescriptor Descriptor { get; set; }

        /// <summary>
        /// Creates a new material from the specified descriptor.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="descriptor">The material descriptor.</param>
        /// <returns>An instance of a <see cref="Material"/>.</returns>
        /// <exception cref="System.ArgumentNullException">descriptor</exception>
        /// <exception cref="System.InvalidOperationException">If an error occurs with the material description</exception>
        public static Material New(GraphicsDevice device, MaterialDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException("descriptor");
            var context = new MaterialGeneratorContext(new Material())
            {
                GraphicsProfile = device.Features.RequestedProfile,
            };
            var result = MaterialGenerator.Generate(descriptor, context, string.Format("{0}:RuntimeMaterial", descriptor.MaterialId));

            if (result.HasErrors)
            {
                throw new InvalidOperationException(string.Format("Error when creating the material [{0}]", result.ToText()));
            }

            return result.Material;
        }
    }
}

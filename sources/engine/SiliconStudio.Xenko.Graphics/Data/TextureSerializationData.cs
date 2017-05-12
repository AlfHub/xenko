﻿// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Diagnostics;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Streaming;

namespace SiliconStudio.Xenko.Graphics.Data
{
    /// <summary>
    /// Texture serialization data
    /// </summary>
    public sealed class TextureSerializationData
    {
        internal const int Version = 4;

        /// <summary>
        /// The texture image.
        /// </summary>
        public Image Image;

        /// <summary>
        /// Enables/disables texture streaming.
        /// </summary>
        public bool EnableStreaming;

        /// <summary>
        /// The raw bytes with a content storage header description.
        /// </summary>
        public ContentStorageHeader StorageHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureSerializationData"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        public TextureSerializationData([NotNull] Image image)
        {
            Image = image;
            EnableStreaming = false;
            StorageHeader = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureSerializationData"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="enableStreaming">Enables/disables texture streaming</param>
        /// <param name="storageHeader">Streaming storage data</param>
        public TextureSerializationData([NotNull] Image image, bool enableStreaming, ContentStorageHeader storageHeader)
        {
            Image = image;
            EnableStreaming = enableStreaming;
            StorageHeader = storageHeader;
        }

        /// <summary>
        /// Saves this instance to a stream.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        public void Write(SerializationStream stream)
        {
            stream.Write(EnableStreaming);
            if (EnableStreaming)
            {
                // Write image header
                ImageHelper.ImageDescriptionSerializer.Serialize(ref Image.Description, ArchiveMode.Serialize, stream);

                // Write storage header
                Debug.Assert(StorageHeader != null);
                StorageHeader.Write(stream);
            }
            else
            {
                // Write whole image (old texture content serialization)
                Image.Save(stream.NativeStream, ImageFileType.Xenko);
            }
        }
    }
}

﻿using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MediaBrowser.Controller.Drawing
{
    /// <summary>
    /// Provides a base image processor class that plugins can use to process images as they are being writen to http responses
    /// Since this is completely modular with MEF, a plugin only needs to have a subclass in their assembly with the following attribute on the class:
    /// [Export(typeof(BaseImageProcessor))]
    /// This will require a reference to System.ComponentModel.Composition
    /// </summary>
    public abstract class BaseImageProcessor
    {
        /// <summary>
        /// Processes an image for a BaseEntity
        /// </summary>
        /// <param name="originalImage">The original Image, before re-sizing</param>
        /// <param name="bitmap">The bitmap holding the original image, after re-sizing</param>
        /// <param name="graphics">The graphics surface on which the output is drawn</param>
        /// <param name="entity">The entity that owns the image</param>
        /// <param name="imageType">The image type</param>
        /// <param name="imageIndex">The image index (currently only used with backdrops)</param>
        public abstract void ProcessImage(Image originalImage, Bitmap bitmap, Graphics graphics, BaseEntity entity, ImageType imageType, int imageIndex);

        /// <summary>
        /// If true, the image output format will be forced to png, resulting in an output size that will generally run larger than jpg
        /// If false, the original image format is preserved.
        /// </summary>
        public virtual bool RequiresTransparency
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if the image processor is configured to process the specified entity, image type and image index
        /// This will aid http response caching so that we don't invalidate image caches when we don't have to
        /// </summary>
        public abstract bool IsConfiguredToProcess(BaseEntity entity, ImageType imageType, int imageIndex);

        /// <summary>
        /// This is used for caching purposes, since a configuration change needs to invalidate a user's image cache
        /// If the image processor is hosted within a plugin then this should be the plugin ConfigurationDateLastModified 
        /// </summary>
        public abstract DateTime ProcessingConfigurationDateLastModifiedUtc { get; }
    }

    /// <summary>
    /// This is demo-ware and should be deleted eventually
    /// </summary>
    //[Export(typeof(BaseImageProcessor))]
    public class MyRoundedCornerImageProcessor : BaseImageProcessor
    {
        public override void ProcessImage(Image originalImage, Bitmap bitmap, Graphics graphics, BaseEntity entity, ImageType imageType, int imageIndex)
        {
            var CornerRadius = 20;

            graphics.Clear(Color.Transparent);

            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddArc(0, 0, CornerRadius, CornerRadius, 180, 90);
                gp.AddArc(0 + bitmap.Width - CornerRadius, 0, CornerRadius, CornerRadius, 270, 90);
                gp.AddArc(0 + bitmap.Width - CornerRadius, 0 + bitmap.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                gp.AddArc(0, 0 + bitmap.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);

                graphics.SetClip(gp);
                graphics.DrawImage(originalImage, 0, 0, bitmap.Width, bitmap.Height);
            }
        }

        public override bool RequiresTransparency
        {
            get
            {
                return true;
            }
        }

        private static DateTime testDate = DateTime.UtcNow;

        public override DateTime ProcessingConfigurationDateLastModifiedUtc
        {
            get
            {
                // This will result in a situation where images are only cached throughout a server session, but again, this is a prototype
                return testDate;
            }
        }

        public override bool IsConfiguredToProcess(BaseEntity entity, ImageType imageType, int imageIndex)
        {
            return true;
        }
    }
}

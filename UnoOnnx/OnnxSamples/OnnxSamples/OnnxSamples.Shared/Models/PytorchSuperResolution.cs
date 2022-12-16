﻿using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using SkiaSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnnxSamples.Models
{
    internal class PytorchSuperResolution
    {
        #region Variable(s)

        const int DimBatchSize = 1;
        const int DimNumberOfChannels = 1;
        const int ImageSizeX = 224;
        const int ImageSizeY = 224;
        const string ModelInputName = "input";
        const string ModelOutputName = "output";

        byte[] _model;
        byte[] _sampleImage;
        List<string> _labels;
        InferenceSession _session;
        Task _initTask;

        public string[] EmbeddedResources { get; } = typeof(MainPage).Assembly.GetManifestResourceNames();

        #endregion

        #region Method(s)

        public async Task<byte[]> GetSampleImageAsync(string filename)
        {
            await InitAsync(filename).ConfigureAwait(false);
            var assembly = GetType().Assembly;
            // Get sample image
            var imageResource = EmbeddedResources.First(item => item.EndsWith(filename));
            using var sampleImageStream = assembly.GetManifestResourceStream(imageResource);
            using var sampleImageMemoryStream = new MemoryStream();

            sampleImageStream.CopyTo(sampleImageMemoryStream);
            _sampleImage = sampleImageMemoryStream.ToArray();
            return _sampleImage;
        }

        Task InitAsync(string filename = "fish.jpg")
        {
            if (_initTask == null || _initTask.IsFaulted)
                _initTask = InitTask(filename);

            return _initTask;
        }

        async Task InitTask(string filename)
        {
            var assembly = GetType().Assembly;

            // Get model
            var modelResource = EmbeddedResources.First(item => item.EndsWith("super_resolution.onnx"));
            using var modelStream = assembly.GetManifestResourceStream(modelResource);
            using var modelMemoryStream = new MemoryStream();

            modelStream.CopyTo(modelMemoryStream);
            _model = modelMemoryStream.ToArray();

            // Create InferenceSession (runtime representation of the model with optional SessionOptions)
            // This can be reused for multiple inferences to avoid unnecessary allocation/dispose overhead
            // https://onnxruntime.ai/docs/api/csharp-api#inferencesession
            // https://onnxruntime.ai/docs/api/csharp-api#sessionoptions
            _session = new InferenceSession(_model);

            // Get sample image
            var imageResource = EmbeddedResources.First(item => item.EndsWith(filename));
            using var sampleImageStream = assembly.GetManifestResourceStream(imageResource);
            using var sampleImageMemoryStream = new MemoryStream();

            sampleImageStream.CopyTo(sampleImageMemoryStream);
            _sampleImage = sampleImageMemoryStream.ToArray();
        }

        public async Task<(bool, byte[])> GetSuperResolutionImage(byte[] image, string filename)
        {
            await InitAsync(filename).ConfigureAwait(false);
            using var sourceBitmap = SKBitmap.Decode(image);
            var pixels = sourceBitmap.Bytes;
            SKBitmap grayScaleBitmap = sourceBitmap;

            //Preprocessing
            if (sourceBitmap.Width != ImageSizeX || sourceBitmap.Height != ImageSizeY)
            {
                float ratio = (float)Math.Min(ImageSizeX, ImageSizeY) / Math.Min(sourceBitmap.Width, sourceBitmap.Height);

                using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(
                    (int)(ratio * sourceBitmap.Width),
                    (int)(ratio * sourceBitmap.Height)),
                    SKFilterQuality.Medium);

                var horizontalCrop = scaledBitmap.Width - ImageSizeX;
                var verticalCrop = scaledBitmap.Height - ImageSizeY;
                var leftOffset = horizontalCrop == 0 ? 0 : horizontalCrop / 2;
                var topOffset = verticalCrop == 0 ? 0 : verticalCrop / 2;

                var cropRect = SKRectI.Create(
                    new SKPointI(leftOffset, topOffset),
                    new SKSizeI(ImageSizeX, ImageSizeY));

                using SKImage currentImage = SKImage.FromBitmap(scaledBitmap);
                using SKImage croppedImage = currentImage.Subset(cropRect);
                using SKBitmap croppedBitmap = SKBitmap.FromImage(croppedImage);
                croppedBitmap.CopyTo(grayScaleBitmap, SKColorType.Gray8);
                pixels = grayScaleBitmap.Bytes;
            }

            var bytesPerPixel = sourceBitmap.BytesPerPixel;
            var rowLength = ImageSizeX * bytesPerPixel;
            var channelLength = ImageSizeX * ImageSizeY;
            var channelData = new float[channelLength];
            channelData = pixels.Select(pixel => pixel / 255f).ToArray();
           

            var input = new DenseTensor<float>(channelData, new[] { DimBatchSize, DimNumberOfChannels, ImageSizeX, ImageSizeY });

            using var results = _session.Run(new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(ModelInputName, input) });

            var output = results.FirstOrDefault(i => i.Name == ModelOutputName);

            if (output == null)
                return (false, image);

            var outputData = output.AsTensor<float>().ToArray();
            var outputBytes = outputData.Select(pixel => (byte)(pixel * 255)).ToArray();
          //  using var outputBitmap = SKBitmap.Decode(outputBytes);
           // var outputImage = SKImage.FromBitmap(outputBitmap);
            return (true, outputBytes);
        }

        #endregion

    }
}

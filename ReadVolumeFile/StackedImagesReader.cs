using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ReadVolumeFile
{
    class StackedImagesReader
    {
        private readonly FileInfo _StackedFileInfo;

        public StackedImagesReader(string filePath)
        {
            _StackedFileInfo = new FileInfo(filePath);

            if (!_StackedFileInfo.Exists)
            {
                throw new ArgumentException("File path does not exist");
            }
        }

        public IEnumerable<BitmapSource> ReadAsSliceImages(int skipHeader, int width, int height, int depth, int numberOfBytesPerPixel)
        {
            List<BitmapSource> images = new List<BitmapSource>();

            using (var fileReader = new FileStream(_StackedFileInfo.FullName, FileMode.Open))
            {
                fileReader.Seek(skipHeader, SeekOrigin.Begin);
                byte[] array = new byte[width * height * numberOfBytesPerPixel];
                
                for (int i = 0; i < depth; i++)
                {
                    fileReader.Read(array, 0, array.Length);

                    var image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray16, 
                        BitmapPalettes.Gray16, array, width * numberOfBytesPerPixel);
                    image.Freeze();
                    images.Add(image);
                }
            }

            return images;
        }

        public IEnumerable<byte[]> ReadAsLazySliceData(int skipHeader, int width, int height, int depth)
        {
            using (var fileReader = new FileStream(_StackedFileInfo.FullName, FileMode.Open))
            {
                fileReader.Seek(skipHeader, SeekOrigin.Begin);
                byte[] array = new byte[width * height * 2];
                for (int i = 0; i < depth; i++)
                {
                    fileReader.Read(array, 0, array.Length);
                    yield return array;
                }
            }
        }

        public ushort[,,] ReadUsing3DArray(int skipHeader, int width, int height, int depth)
        {
            ushort[,,] volume3DData = new ushort[width, height, depth];

            using (var fileReader = new FileStream(_StackedFileInfo.FullName, FileMode.Open))
            {
                using (var reader = new BinaryReader(fileReader))
                {
                    fileReader.Seek(skipHeader, SeekOrigin.Begin);

                    for (int volDepth = 0; volDepth < depth; volDepth++)
                    {
                        for (int volHeight = 0; volHeight < height; volHeight++)
                        {
                            for (int volWidth = 0; volWidth < width; volWidth++)
                            {
                                volume3DData[volWidth, volHeight, volDepth] = reader.ReadUInt16();
                            }
                        }
                    }
                }
            }
            return volume3DData;
        }

        public unsafe ushort* ReadUsingAllocHGlobal(int skipHeader, int width, int height, int depth)
        {
            var allocatedData = Marshal.AllocHGlobal(width * height * depth * 2);
            ushort* volumeDataPtr = (ushort*)allocatedData.ToPointer();

            using (var fileReader = new FileStream(_StackedFileInfo.FullName, FileMode.Open))
            {
                using (var reader = new BinaryReader(fileReader))
                {
                    fileReader.Seek(skipHeader, SeekOrigin.Begin);

                    for (int volDepth = 0; volDepth < depth; volDepth++)
                    {
                        for (int volHeight = 0; volHeight < height; volHeight++)
                        {
                            for (int volWidth = 0; volWidth < width; volWidth++)
                            {
                                volumeDataPtr[volWidth + volHeight * width + width * height * volDepth] = reader.ReadUInt16();
                            }
                        }
                    }
                }
            }
            return volumeDataPtr;
        }

        public unsafe ushort* ReadUsingNativeAlloc(int skipHeader, int width, int height, int depth)
        {
            ushort* volumeDataPtr = stackalloc ushort[width * height * depth * 2];

            using (var fileReader = new FileStream(_StackedFileInfo.FullName, FileMode.Open))
            {
                using (var reader = new BinaryReader(fileReader))
                {
                    fileReader.Seek(skipHeader, SeekOrigin.Begin);

                    for (int volDepth = 0; volDepth < depth; volDepth++)
                    {
                        for (int volHeight = 0; volHeight < height; volHeight++)
                        {
                            for (int volWidth = 0; volWidth < width; volWidth++)
                            {
                                volumeDataPtr[volWidth + volHeight * width + width * height * volDepth] = reader.ReadUInt16();
                            }
                        }
                    }
                }
            }
            return volumeDataPtr;
        }

        public ushort[][] ReadVolumeUsingJaggedArray(int skipHeader, int width, int height, int depth)
        {
            ushort[][] volumeData = new ushort[depth][];

            using (var fileReader = new FileStream(_StackedFileInfo.FullName, FileMode.Open))
            {
                using (var reader = new BinaryReader(fileReader))
                {
                    fileReader.Seek(skipHeader, SeekOrigin.Begin);

                    for (int i = 0; i < depth; i++)
                    {
                        volumeData[i] = new ushort[width * height];
                        for (int j = 0; j < width * height; j++)
                        {
                            volumeData[i][j] = reader.ReadUInt16();
                        }
                    }
                }
            }
            return volumeData;
        }

        public ushort[][] ReadVolumeUsingMemoryMappedFile(int skipHeader, int width, int height, int depth)
        {
            ushort[][] volumeData = new ushort[depth][];

            using (var memoryMappedFile = MemoryMappedFile.CreateFromFile(_StackedFileInfo.FullName, FileMode.Open,
                _StackedFileInfo.Name, skipHeader + width * height * depth * 2, MemoryMappedFileAccess.Read))
            {
                var stream = memoryMappedFile.CreateViewStream();
                stream.Seek(skipHeader, SeekOrigin.Begin);
                using (var reader = new BinaryReader(memoryMappedFile.CreateViewStream()))
                {
                    for (int i = 0; i < depth; i++)
                    {
                        volumeData[i] = new ushort[width * height];
                        for (int j = 0; j < width * height; j++)
                        {
                            volumeData[i][j] = reader.ReadUInt16();
                        }
                    }
                }
            }
            return volumeData;
        }

        public unsafe ushort* ReadVolumeUsingPInvoke(int skipHeader, int width, int height, int depth)
        {
            IntPtr memoryPtr = default;
            IntPtr heapPtr = NativeMethods.GetProcessHeap();

            fixed (long* ptr = new long[1])
            {
                *ptr = width * height * depth * 2;
                var memoryPointer = new UIntPtr(ptr);
                memoryPtr = NativeMethods.HeapAlloc(heapPtr, (uint)NativeMethods.DwFlags.HEAP_ZERO_MEMORY, memoryPointer);
            }
            
            ushort* volumeDataPtr = (ushort*)memoryPtr.ToPointer();
            using (var fileReader = new FileStream(_StackedFileInfo.FullName, FileMode.Open))
            {
                using (var reader = new BinaryReader(fileReader))
                {
                    fileReader.Seek(skipHeader, SeekOrigin.Begin);

                    for (int volDepth = 0; volDepth < depth; volDepth++)
                    {
                        for (int volHeight = 0; volHeight < height; volHeight++)
                        {
                            for (int volWidth = 0; volWidth < width; volWidth++)
                            {
                                *volumeDataPtr = reader.ReadUInt16();
                                volumeDataPtr++;
                            }
                        }
                    }
                }
            }
            volumeDataPtr = (ushort*)memoryPtr.ToPointer();
            return volumeDataPtr;
        }
    }
}
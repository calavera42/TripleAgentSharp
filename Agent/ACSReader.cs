using Agent.Structures;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Agent
{
    internal class ACSReader
    {
        private const uint _signature = 0xABCDABC3;

        private readonly ACSLocator _charInfo;
        private readonly ACSLocator _animInfo;
        private readonly ACSLocator _imgInfo;
        private readonly ACSLocator _audioInfo;

        private readonly Dictionary<string, ACSLocator> _animations = [];

        private readonly BinaryReader _stream;

        private readonly CharacterInfo _info;

        public ACSReader(Stream s)
        {
            _stream = new BinaryReader(s);

            if (_signature != _stream.ReadUInt32())
                throw new FormatException($"A assinatura não bate. Esperado: 0xABCDABC3");

            _charInfo = ReadLocator();
            _animInfo = ReadLocator();
            _imgInfo = ReadLocator();
            _audioInfo = ReadLocator();

            _info = ReadCharInfo();

            LoadAnimations();
        }

        private void LoadAnimations()
        {
            _stream.Seek(_animInfo.Offset, SeekOrigin.Begin);

            uint animCount = _stream.ReadUInt32();

            for (int i = 0; i < animCount; i++)
            {
                string animName = _stream.ReadCString();
                ACSLocator animInfo = ReadLocator();

                _animations.Add(animName.ToLowerInvariant(), animInfo);
            }
        }

        private ACSLocator ReadLocator() => new() { Offset = _stream.ReadUInt32(), Size = _stream.ReadUInt32() };

        public CharacterInfo ReadCharInfo()
        {
            _stream.Seek(_charInfo.Offset, SeekOrigin.Begin);

            CharacterInfo output = new();
            output.MinorVersion = _stream.ReadUInt16();
            output.MajorVersion = _stream.ReadUInt16();
            output.LocalizedInfo = ReadLocalizedInfo(ReadLocator());
            output.GUID = _stream.ReadGuid();
            output.Width = _stream.ReadUInt16();
            output.Height = _stream.ReadUInt16();
            byte transparentColorIndex = _stream.ReadByte();
            output.Flags = _stream.ReadUInt32();
            output.AnimationMajorVersion = _stream.ReadUInt16();
            output.AnimationMinorVersion = _stream.ReadUInt16();

            if ((output.Flags & CharacterFlags.VoiceEnabled) > 0)
                output.VoiceInfo = ReadVoiceInfo();

            if ((output.Flags & CharacterFlags.BalloonEnabled) > 0)
                output.BalloonInfo = ReadBalloonInfo();

            uint colorTableSize = _stream.ReadUInt32();

            using (Bitmap b = new((int)colorTableSize, 1))
            {
                for (int i = 0; i < colorTableSize; i++)
                    b.SetPixel(i, 0, ReadColor());
                b.ConvertFormat(PixelFormat.Format8bppIndexed);
                output.Palette = b.Palette;
            }

            if (_stream.ReadBoolean()) // ler o trayicon B-)
                output.TrayIcon = ReadTrayIcon();

            output.StateInfo = ReadStateInfo();
            output.TransparentColor = output.Palette.Entries[transparentColorIndex];

            return output;
        }

        public Bitmap ReadImage(uint index)
        {
            _stream.Seek(_imgInfo.Offset, SeekOrigin.Begin);

            if (index > _stream.ReadUInt32())
                throw new IndexOutOfRangeException("O índice solicitado não existe.");

            _stream.Seek((ACSLocator.SizeOf + sizeof(uint)) * index, SeekOrigin.Current);

            ACSLocator acsLocator = ReadLocator();

            _stream.Seek(acsLocator.Offset, SeekOrigin.Begin);

            ImageInfo info = new()
            {
                Unknown = _stream.ReadByte(),
                Width = _stream.ReadUInt16(),
                Height = _stream.ReadUInt16(),
                Compressed = _stream.ReadBoolean(),
                ImageData = ReadDataBlock(),
            };

            uint uncompressedImageSize = (uint)(info.Width * info.Height);
            byte[] imageData = info.ImageData.Data;

            if (info.Compressed)
                imageData = Decompress(imageData, uncompressedImageSize);

            // converter para bitmap
            Bitmap output = new(info.Width, info.Height, PixelFormat.Format8bppIndexed);

            Rectangle rect = new(0, 0, info.Width, info.Height);

            BitmapData bmpData = output.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            IntPtr ptrToPixel = bmpData.Scan0;

            Parallel.For(0, imageData.Length, i => Marshal.Copy(imageData, i, bmpData.Scan0 + i, 1));

            output.UnlockBits(bmpData);
            output.RotateFlip(RotateFlipType.RotateNoneFlipY);
            output.Palette = _info.Palette;

            output.MakeTransparent(_info.TransparentColor);
            //---------------------
            return output;
        }

        public MemoryStream ReadAudioStream(uint index)
        {
            _stream.Seek(_audioInfo.Offset, SeekOrigin.Begin);

            if (index > _stream.ReadUInt32())
                throw new IndexOutOfRangeException("O índice solicitado não existe.");

            _stream.Seek((ACSLocator.SizeOf + sizeof(uint)) * index, SeekOrigin.Current);

            ACSLocator audioData = ReadLocator();

            _stream.Seek(audioData.Offset, SeekOrigin.Begin);

            return new MemoryStream(_stream.ReadBytes((int)audioData.Size));
        }

        public AnimationInfo ReadAnimation(string name)
        {
            if (!_animations.TryGetValue(name.ToLowerInvariant(), out ACSLocator value))
                return new();

            _stream.Seek(value.Offset, SeekOrigin.Begin);

            string animationName = _stream.ReadCString();
            TransitionType transitionType = (TransitionType)_stream.ReadByte();
            string returnAnimation = _stream.ReadCString();
            FrameInfo[] animationFrames = ReadFrameInfo();

            return new(animationName, transitionType, returnAnimation, animationFrames);
        }

        public string[] GetAnimationNames() => _animations.Keys.ToArray();

        private Dictionary<string, string[]> ReadStateInfo()
        {
            ushort listCount = _stream.ReadUInt16();
            Dictionary<string, string[]> output = new();

            for (int i = 0; i < listCount; i++)
            {
                string name = _stream.ReadCString().ToLowerInvariant();
                ushort animationCount = _stream.ReadUInt16();
                string[] animations = new string[animationCount];

                for (int j = 0; j < animationCount; j++)
                    animations[j] = _stream.ReadCString().ToLowerInvariant();

                output.Add(name, animations);
            }

            return output;
        }

        private LocalizedInfo[] ReadLocalizedInfo(ACSLocator pos)
        {
            long streamPos = _stream.GetPos();
            _stream.Seek(pos.Offset, SeekOrigin.Begin);

            int listLength = _stream.ReadUInt16();

            LocalizedInfo[] output = new LocalizedInfo[listLength];

            for (int i = 0; i < listLength; i++)
                output[i] = new LocalizedInfo()
                {
                    LangID = _stream.ReadUInt16(),
                    CharacterName = _stream.ReadCString(),
                    CharacterDescription = _stream.ReadCString(),
                    CharacterExtraData = _stream.ReadCString(),
                };

            _stream.Seek(streamPos, SeekOrigin.Begin);

            return output;
        }

        /*
         * Explicação do TrayIcon:
         * 
         * A explicação fornecida pela documentação é incorreta.
         * 
         * Na documentação é especificado o seguinte:
         * 
         * DATA TYPE        | QUANTITY | DESCRIPTION
         * BITMAPINFOHEADER | 1        | Icon Header
         * RGBQUAD          | variable | Color Table
         * BYTE             | variable | XOR Mask Bits
         * BYTE             | variable | AND Mask Bits 
         * 
         * A realidade é que a estrutura ICONIMAGE só fornece um array de bytes.
         * O primeiro ICONIMAGE se trata da máscara e o segundo da informação de cores.
         * 
         * O código abaixo foi parcialmente inspirado na implementação do DoubleAgent.
         * ---------------------------------------------------------
         * 
         * Quando a quantidade de cores especificada (BitmapInfoHeader.ClrUsed) for zero,
         * a quantidade de cores usada será 2^bitcount.
         */
        public Icon ReadTrayIcon()
        {
            int iconSize = _stream.ReadInt32();
            BitmapInfoHeader maskInfoHeader;
            BitmapInfoHeader colorInfoHeader;
            byte[] maskBytes;
            byte[] colorBytes;

            maskInfoHeader = ReadDIBHeader();
            _stream.Seek(sizeof(uint) * 2, SeekOrigin.Current); // não faço a menor ideia do porque disso
            maskBytes = _stream.ReadBytes((int)(iconSize - maskInfoHeader.Size - sizeof(uint) * 2));

            iconSize = _stream.ReadInt32();
            colorInfoHeader = ReadDIBHeader();
            colorBytes = _stream.ReadBytes((int)(iconSize - colorInfoHeader.Size));

            MemoryStream icoFile = new();

            colorInfoHeader.ImageSize = (uint)(maskBytes.Length + colorBytes.Length) / 2;

            using (BinaryWriter bw = new(icoFile, System.Text.Encoding.Default, true))
            {
                bw.Write((ushort)0); // sempre 0
                bw.Write((ushort)1); // tipo de imagem, 1 para .ICO; 2 para .CUR
                bw.Write((ushort)1); // número de imagens no arquivo
                bw.Write((byte)colorInfoHeader.Width);
                bw.Write((byte)colorInfoHeader.Height);
                bw.Write((byte)colorInfoHeader.BitCount); // aqui era pra ser o número de cores, mas aparentemente o número real é o bitcount (provavelmente por causa da regra das cores usadas serem zero etc e tal)
                bw.Write((byte)0);
                bw.Write((ushort)0);
                bw.Write((ushort)0);
                bw.Write((uint)(maskBytes.Length + colorBytes.Length) + colorInfoHeader.Size); // tamanho do ícone em bytes (cabeçalho + color bytes + mask bytes)
                bw.Write((uint)bw.BaseStream.Position + sizeof(uint)); // índice da informação do ícone no arquivo.

                bw.Write(colorInfoHeader.Size);
                bw.Write(colorInfoHeader.Width);
                bw.Write(colorInfoHeader.Height * 2); // altura dupla, (colorbytes + maskbytes)
                bw.Write(colorInfoHeader.Planes);
                bw.Write(colorInfoHeader.BitCount);
                bw.Write(colorInfoHeader.Compression);
                bw.Write(colorInfoHeader.ImageSize);
                bw.Write(colorInfoHeader.XPelsPerMeter);
                bw.Write(colorInfoHeader.YPelsPerMeter);
                bw.Write(colorInfoHeader.ClrUsed);
                bw.Write(colorInfoHeader.ClrImportant);

                bw.Write(colorBytes);
                bw.Write(maskBytes);

                icoFile.Seek(0, SeekOrigin.Begin);
            }

            return new Icon(icoFile);
        }

        private BitmapInfoHeader ReadDIBHeader()
        {
            return new()
            {
                Size = _stream.ReadUInt32(),
                Width = _stream.ReadInt32(),
                Height = _stream.ReadInt32(),
                Planes = _stream.ReadUInt16(),
                BitCount = _stream.ReadUInt16(),
                Compression = _stream.ReadUInt32(),
                ImageSize = _stream.ReadUInt32(),
                XPelsPerMeter = _stream.ReadInt32(),
                YPelsPerMeter = _stream.ReadInt32(),
                ClrUsed = _stream.ReadUInt32(),
                ClrImportant = _stream.ReadUInt32()
            };
        }

        private VoiceInfo ReadVoiceInfo()
        {
            VoiceInfo output = new()
            {
                TTSEngineID = _stream.ReadGuid(),
                TTSModeID = _stream.ReadGuid(),
                Speed = _stream.ReadUInt32(),
                Pitch = _stream.ReadUInt16(),
                ExtraData = _stream.ReadBoolean()
            };

            if (output.ExtraData)
            {
                output.LangID = _stream.ReadUInt16();
                output.LanguageDialect = _stream.ReadCString();
                output.Gender = _stream.ReadUInt16();
                output.Age = _stream.ReadUInt16();
                output.Style = _stream.ReadCString();
            }

            return output;
        }

        private BalloonInfo ReadBalloonInfo()
        {
            return new()
            {
                NumberTextLines = _stream.ReadByte(),
                CharsPerLine = _stream.ReadByte(),
                ForegroundColor = ReadColor(),
                BackgroundColor = ReadColor(),
                BorderColor = ReadColor(),
                FontName = _stream.ReadCString(),
                FontHeight = _stream.ReadInt32(),
                FontWeight = _stream.ReadInt32(),
                Italic = _stream.ReadBoolean(),
                Reserved = _stream.ReadByte()
            };
        }

        private Color ReadColor() // por algum motivo místico, a microsoft decidiu que o RGBQUAD deva ser estruturado ao contrário... BGRA ao invés de RGBA (A sendo o Reserved)
        {
            byte blue = _stream.ReadByte();
            byte green = _stream.ReadByte();
            byte red = _stream.ReadByte();
            byte reserved = _stream.ReadByte();

            return Color.FromArgb(red, green, blue);
        }

        private FrameInfo[] ReadFrameInfo()
        {
            ushort frameCount = _stream.ReadUInt16();
            FrameInfo[] frameInfoList = new FrameInfo[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                FrameInfo frame = new()
                {
                    Layers = ReadFrameImageList(),
                    AudioIndex = _stream.ReadInt16(),
                    Duration = _stream.ReadUInt16(),
                    ExitFrameIndex = _stream.ReadInt16(),
                    Branches = ReadBranchInfoList(),
                };
                frame.MouthOverlays = ReadOverlayInfoList();

                frameInfoList[i] = frame;
            }

            return frameInfoList;
        }

        private FrameImage[] ReadFrameImageList()
        {
            ushort listCount = _stream.ReadUInt16();
            FrameImage[] output = new FrameImage[listCount];

            for (int i = 0; i < listCount; i++)
            {
                FrameImage image = new()
                {
                    ImageIndex = _stream.ReadUInt32(),
                    OffsetX = _stream.ReadInt16(),
                    OffsetY = _stream.ReadInt16()
                };

                output[i] = image;
            }
            return output;
        }

        private BranchInfo[] ReadBranchInfoList()
        {
            byte listCount = _stream.ReadByte();
            BranchInfo[] branchInfoList = new BranchInfo[listCount];

            for (int i = 0; i < listCount; i++)
            {
                BranchInfo info = new()
                {
                    TargetFrameIndex = _stream.ReadUInt16(),
                    Probability = _stream.ReadUInt16()
                };

                branchInfoList[i] = info;
            }

            return branchInfoList;
        }

        private OverlayInfo[] ReadOverlayInfoList()
        {
            byte listCount = _stream.ReadByte();
            OverlayInfo[] overlayInfoList = new OverlayInfo[listCount];

            for (int i = 0; i < listCount; i++)
            {
                OverlayInfo info = new()
                {
                    OverlayType = (MouthOverlay)_stream.ReadByte(),
                    ReplaceTopFrameImage = _stream.ReadBoolean(),
                    ImageIndex = _stream.ReadUInt16(),
                    Unknown = _stream.ReadByte(),
                    HasRegionData = _stream.ReadBoolean(),
                    OffsetX = _stream.ReadInt16(),
                    OffsetY = _stream.ReadInt16(),
                    Width = _stream.ReadUInt16(),
                    Height = _stream.ReadUInt16()
                };

                if (info.HasRegionData)
                {
                    _stream.ReadInt32();
                    info.RegionData = ReadRgnData();
                }
                overlayInfoList[i] = info;
            }

            return overlayInfoList;
        }

        private RgnData ReadRgnData()
        {
            RgnData output = new()
            {
                Size = _stream.ReadUInt32(),
                RegionType = _stream.ReadUInt32(),
                RectCount = _stream.ReadUInt32(),
                RectBufferSize = _stream.ReadUInt32(),
                Region = ReadRect(),
            };

            output.Rects = new Rect[output.RectCount];

            for (int i = 0; i < output.RectCount; i++)
                output.Rects[i] = ReadRect();

            return output;
        }

        private Rect ReadRect()
        {
            return new()
            {
                Left = _stream.ReadInt32(),
                Top = _stream.ReadInt32(),
                Right = _stream.ReadInt32(),
                Bottom = _stream.ReadInt32(),
            };
        }

        private Datablock ReadDataBlock()
        {
            Datablock data = new();
            data.Size = _stream.ReadUInt32();
            data.Data = _stream.ReadBytes((int)data.Size);

            return data;
        }

        private byte[] Decompress(byte[] info, uint outputSize, bool image = false)
        {
            byte[] bitCountTable = [
                6, 9, 12, 20
            ];

            ushort[] valueSumTable =
            [
                1, 65, 577, 4673
            ];

            byte[] uncompressedBuffer = new byte[outputSize];
            uint index = 0;

            using (MemoryStream ms = new(info))
            {
                BitReader br = new(ms);
                br.ReadBits(8);

                while (true)
                {
                    if (br.ReadBit() > 0) // pelo menos 2 bytes comprimidos
                    {
                        ushort countOfBytesToDecode = 2;
                        byte offsetSequentialBits = CountSequencialBits(br, 3);
                        byte offsetBitCount = bitCountTable[offsetSequentialBits];
                        uint offset = br.ReadBits(offsetBitCount);

                        if (offsetSequentialBits == 3) // 20 bits lidos
                            if (offset == 0x000FFFFF) // fim dos dados
                                break;
                            else
                                countOfBytesToDecode++;

                        offset += valueSumTable[offsetSequentialBits];

                        byte decBytesSequentialBits = CountSequencialBits(br, 11);

                        if (decBytesSequentialBits == 11 && br.ReadBit() > 0)
                            throw new Exception("O 12º bit da sequência é 1.");

                        if (decBytesSequentialBits != 0)
                        {
                            countOfBytesToDecode += (ushort)((1 << decBytesSequentialBits) - 1); // valor de bits sequenciais. 0b111 = 2^3 - 1
                            countOfBytesToDecode += (ushort)br.ReadBits(decBytesSequentialBits);
                        }

                        for (int i = 0; i < countOfBytesToDecode; i++)
                        {
                            uncompressedBuffer[index] = uncompressedBuffer[index - offset];
                            index++;
                        }
                    }
                    else
                    {
                        uncompressedBuffer[index] = (byte)br.ReadBits(8);
                        index++;
                    }
                }
            }

            return uncompressedBuffer;
        }

        private byte CountSequencialBits(BitReader br, int maxBits)
        {
            byte sequencialBits = 0;
            for (int i = 0; i < maxBits; i++)
            {
                if (br.ReadBit() == 0)
                    break;
                sequencialBits++;
            }

            return sequencialBits;
        }
    }
}

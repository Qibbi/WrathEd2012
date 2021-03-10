#include "RefPack.h"

void SAGECOMPRESSION_DLL_EXPORT Decompress(unsigned char* buffer, const char* filename, uint fileOffset, uint length, uint offset)
{
	uint refPackBufferLength = (MIN_REFPACK_BUFFER > length) ? MIN_REFPACK_BUFFER : length;
	unsigned char* refPackBuffer = new unsigned char[refPackBufferLength];
	unsigned char* preBuffer = new unsigned char[REFPACK_PREBUFFER];
	unsigned char* preBufferOffset = preBuffer;
	FILE* file = fopen(filename, "rb");
	fseek(file, fileOffset, SEEK_SET);
	fread(preBuffer, 1, REFPACK_PREBUFFER, file);
	preBuffer += 2 + ((*preBuffer & 0x80) ? 0x04 : 0x03) * ((*preBuffer & 0x01) ? 0x02 : 0x01);
	unsigned char refPack0 = 0;
	unsigned char refPack1 = 0;
	unsigned char refPack2 = 0;
	unsigned char refPack3 = 0;
	uint countPart1 = 0;
	uint countPart2 = 0;
	uint position = 0;
	uint refPackOffset = 0;
	while (true)
	{
		if (preBuffer >= preBufferOffset + REFPACK_PREBUFFER - 0x80)
		{
			fseek(file, 0 - REFPACK_PREBUFFER - (preBuffer - preBufferOffset), SEEK_CUR);
			preBuffer = preBufferOffset;
			fread(preBuffer, 1, REFPACK_PREBUFFER, file);
		}
		refPack0 = *preBuffer++;
		if (!(refPack0 & 0x80))
		{
			refPack1 = *preBuffer++;
			countPart1 = refPack0 & 0x03;
			countPart2 = ((refPack0 & 0x1C) >> 0x02) + 0x02;
			refPackOffset = ((position - 0x01u) - (refPack1 + (refPack0 & 0x60u) << 0x03));
			if (position + countPart1 < offset + length)
			{
				for (uint idx = 0; idx < countPart1; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = *preBuffer++;
				}
			}
			else
			{
				for (uint idx = 0; idx < countPart1; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = *preBuffer++;
					if (position == offset + length)
					{
						if (position % refPackBufferLength < length)
						{
							uint partLength = refPackBufferLength - (offset % refPackBufferLength);
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[(offset % refPackBufferLength) + idx];
								++buffer;
							}
							partLength = length - partLength;
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[idx];
								++buffer;
							}
							delete[] refPackBuffer;
							delete[] preBufferOffset;
							fclose(file);
							return;
						}
					}
				}
			}
			if (position + countPart2 + 1 < offset + length)
			{
				for (uint idx = 0; idx <= countPart2; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
				}
			}
			else
			{
				for (uint idx = 0; idx <= countPart2; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
					if (position == offset + length)
					{
						if (position % refPackBufferLength < length)
						{
							uint partLength = refPackBufferLength - (offset % refPackBufferLength);
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[(offset % refPackBufferLength) + idx];
								++buffer;
							}
							partLength = length - partLength;
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[idx];
								++buffer;
							}
							delete[] refPackBuffer;
							delete[] preBufferOffset;
							fclose(file);
							return;
						}
					}
				}
			}
		}
		else if (!(refPack0 & 0x40))
		{
			refPack1 = *preBuffer++;
			refPack2 = *preBuffer++;
			countPart1 = refPack1 >> 0x06;
			countPart2 = (refPack0 & 0x3F) + 0x03;
			refPackOffset = (position + countPart1 - 0x01) - ((refPack1 & 0x3F) << 0x08) + refPack2;
			if (position + countPart1 < offset + length)
			{
				for (uint idx = 0; idx < countPart1; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = *preBuffer++;
				}
			}
			else
			{
				for (uint idx = 0; idx < countPart1; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = *preBuffer++;
					if (position == offset + length)
					{
						if (position % refPackBufferLength < length)
						{
							uint partLength = refPackBufferLength - (offset % refPackBufferLength);
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[(offset % refPackBufferLength) + idx];
								++buffer;
							}
							partLength = length - partLength;
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[idx];
								++buffer;
							}
							delete[] refPackBuffer;
							delete[] preBufferOffset;
							fclose(file);
							return;
						}
					}
				}
			}
			if (position + countPart2 + 1 < offset + length)
			{
				for (uint idx = 0; idx <= countPart2; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
				}
			}
			else
			{
				for (uint idx = 0; idx <= countPart2; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
					if (position == offset + length)
					{
						if (position % refPackBufferLength < length)
						{
							uint partLength = refPackBufferLength - (offset % refPackBufferLength);
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[(offset % refPackBufferLength) + idx];
								++buffer;
							}
							partLength = length - partLength;
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[idx];
								++buffer;
							}
							delete[] refPackBuffer;
							delete[] preBufferOffset;
							fclose(file);
							return;
						}
					}
				}
			}
		}
		else if (!(refPack0 & 0x20))
		{
			refPack1 = *preBuffer++;
			refPack2 = *preBuffer++;
			refPack3 = *preBuffer++;
			countPart1 = refPack0 & 0x03;
			countPart2 = ((refPack0 & 0x0C) << 0x06) + refPack3 + 0x04;
			refPackOffset = (position + countPart1 - 0x01) - ((refPack0 & 0x10) << 0x0C) + (refPack1 << 0x08) + refPack2;
			if (position + countPart1 < offset + length)
			{
				for (uint idx = 0; idx < countPart1; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = *preBuffer++;
				}
			}
			else
			{
				for (uint idx = 0; idx < countPart1; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = *preBuffer++;
					if (position == offset + length)
					{
						if (position % refPackBufferLength < length)
						{
							uint partLength = refPackBufferLength - (offset % refPackBufferLength);
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[(offset % refPackBufferLength) + idx];
								++buffer;
							}
							partLength = length - partLength;
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[idx];
								++buffer;
							}
							delete[] refPackBuffer;
							delete[] preBufferOffset;
							fclose(file);
							return;
						}
					}
				}
			}
			if (position + countPart2 + 1 < offset + length)
			{
				for (uint idx = 0; idx <= countPart2; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
				}
			}
			else
			{
				for (uint idx = 0; idx <= countPart2; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
					if (position == offset + length)
					{
						if (position % refPackBufferLength < length)
						{
							uint partLength = refPackBufferLength - (offset % refPackBufferLength);
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[(offset % refPackBufferLength) + idx];
								++buffer;
							}
							partLength = length - partLength;
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[idx];
								++buffer;
							}
							delete[] refPackBuffer;
							delete[] preBufferOffset;
							fclose(file);
							return;
						}
					}
				}
			}
		}
		else
		{
			countPart1 = ((refPack0 & 0x1F) << 0x02) + 0x04;
			if (countPart1 > 0x70)
			{
				countPart1 = refPack0 & 0x03;
			}
			if (position + countPart1 < offset + length)
			{
				for (uint idx = 0; idx < countPart1; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = *preBuffer++;
				}
			}
			else
			{
				for (uint idx = 0; idx < countPart1; ++idx)
				{
					refPackBuffer[position++ % refPackBufferLength] = *preBuffer++;
					if (position == offset + length)
					{
						if (position % refPackBufferLength < length)
						{
							uint partLength = refPackBufferLength - (offset % refPackBufferLength);
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[(offset % refPackBufferLength) + idx];
								++buffer;
							}
							partLength = length - partLength;
							for (uint idx = 0; idx < partLength; ++idx)
							{
								*buffer = refPackBuffer[idx];
								++buffer;
							}
							delete[] refPackBuffer;
							delete[] preBufferOffset;
							fclose(file);
							return;
						}
					}
				}
			}
		}
	}
}
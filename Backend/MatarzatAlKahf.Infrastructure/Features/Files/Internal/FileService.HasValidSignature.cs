using System.Security.Cryptography;
using System.Buffers.Binary;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Files.Services;
public sealed partial class FileService
{
    private static bool HasValidSignature(MemoryStream stream, string extension)
    {
        ReadOnlySpan<byte> bytes = stream.GetBuffer().AsSpan(0, checked((int)stream.Length));
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => HasValidJpegStructure(bytes),
            ".png" => HasValidPngStructure(bytes),
            ".webp" => HasValidWebPStructure(bytes),
            ".pdf" => HasValidPdfStructure(bytes),
            _ => false
        };
    }

    private static bool HasValidJpegStructure(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 12
            || bytes[0] != 0xFF
            || bytes[1] != 0xD8
            || bytes[2] != 0xFF
            || bytes[^2] != 0xFF
            || bytes[^1] != 0xD9)
        {
            return false;
        }

        for (int index = 2; index <= bytes.Length - 9; index++)
        {
            if (bytes[index] != 0xFF || !IsStartOfFrameMarker(bytes[index + 1]))
            {
                continue;
            }

            int segmentLength = BinaryPrimitives.ReadUInt16BigEndian(
                bytes.Slice(index + 2, 2));
            if (segmentLength < 8 || index + 2 + segmentLength > bytes.Length)
            {
                return false;
            }

            ushort height = BinaryPrimitives.ReadUInt16BigEndian(
                bytes.Slice(index + 5, 2));
            ushort width = BinaryPrimitives.ReadUInt16BigEndian(
                bytes.Slice(index + 7, 2));
            return height > 0 && width > 0;
        }

        return false;
    }

    private static bool IsStartOfFrameMarker(byte marker) => marker is
        0xC0 or 0xC1 or 0xC2 or 0xC3
        or 0xC5 or 0xC6 or 0xC7
        or 0xC9 or 0xCA or 0xCB
        or 0xCD or 0xCE or 0xCF;

    private static bool HasValidPngStructure(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 45
            || !bytes.StartsWith(
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
        {
            return false;
        }

        int offset = 8;
        bool isFirstChunk = true;
        while (offset <= bytes.Length - 12)
        {
            uint chunkLength = BinaryPrimitives.ReadUInt32BigEndian(
                bytes.Slice(offset, 4));
            if (chunkLength > int.MaxValue)
            {
                return false;
            }

            int length = (int)chunkLength;
            int chunkEnd = offset + 12 + length;
            if (chunkEnd < offset || chunkEnd > bytes.Length)
            {
                return false;
            }

            ReadOnlySpan<byte> chunkType = bytes.Slice(offset + 4, 4);
            if (isFirstChunk)
            {
                if (length != 13
                    || !chunkType.SequenceEqual("IHDR"u8)
                    || BinaryPrimitives.ReadUInt32BigEndian(
                        bytes.Slice(offset + 8, 4)) == 0
                    || BinaryPrimitives.ReadUInt32BigEndian(
                        bytes.Slice(offset + 12, 4)) == 0)
                {
                    return false;
                }

                isFirstChunk = false;
            }

            if (chunkType.SequenceEqual("IEND"u8))
            {
                return length == 0 && chunkEnd == bytes.Length;
            }

            offset = chunkEnd;
        }

        return false;
    }

    private static bool HasValidWebPStructure(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 20
            || !bytes[..4].SequenceEqual("RIFF"u8)
            || !bytes.Slice(8, 4).SequenceEqual("WEBP"u8)
            || BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(4, 4))
                != (uint)(bytes.Length - 8))
        {
            return false;
        }

        ReadOnlySpan<byte> format = bytes.Slice(12, 4);
        return format.SequenceEqual("VP8 "u8)
            || format.SequenceEqual("VP8L"u8)
            || format.SequenceEqual("VP8X"u8);
    }

    private static bool HasValidPdfStructure(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 12 || !bytes.StartsWith("%PDF-"u8))
        {
            return false;
        }

        int trailerStart = Math.Max(0, bytes.Length - 1024);
        return bytes[trailerStart..].IndexOf("%%EOF"u8) >= 0;
    }
}

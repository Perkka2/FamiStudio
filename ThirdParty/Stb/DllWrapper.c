#define STB_RECT_PACK_IMPLEMENTATION
#define STB_TRUETYPE_IMPLEMENTATION

#include "stb_rect_pack.h"
#include "stb_truetype.h"

#ifdef LINUX
#define __stdcall
#endif

#if !defined(_MSC_VER) && !defined(__clang__)
#include <alloca.h>
#endif

int __stdcall StbGetNumberOfFonts(const unsigned char* data)
{
	return stbtt_GetNumberOfFonts(data);
}

int __stdcall StbGetFontOffsetForIndex(const unsigned char* data, int index)
{
	return stbtt_GetFontOffsetForIndex(data, index);
}

void* __stdcall StbInitFont(const unsigned char* data, int offset)
{
	stbtt_fontinfo* info = (stbtt_fontinfo*)malloc(sizeof(stbtt_fontinfo));

	if (stbtt_InitFont(info, data, offset) == 0)
	{
		free(info);
		info = NULL;
	}

	return info;
}

void __stdcall StbFreeFont(void* font)
{
	free(font);
}

float __stdcall StbScaleForPixelHeight(const stbtt_fontinfo* info, float pixels)
{
	return stbtt_ScaleForPixelHeight(info, pixels);
}

float __stdcall StbScaleForMappingEmToPixels(const stbtt_fontinfo* info, float pixels)
{
	return stbtt_ScaleForMappingEmToPixels(info, pixels);
}

void __stdcall StbGetGlyphHMetrics(const stbtt_fontinfo* info, int glyph, int* advanceWidth, int* leftSideBearing)
{
	stbtt_GetGlyphHMetrics(info, glyph, advanceWidth, leftSideBearing);
}

int __stdcall StbGetGlyphKernAdvance(const stbtt_fontinfo* info, int ch1, int ch2)
{
	return stbtt_GetGlyphKernAdvance(info, ch1, ch2);
}

void __stdcall StbGetFontVMetrics(const stbtt_fontinfo* info, int* ascent, int* descent, int* lineGap)
{
	stbtt_GetFontVMetrics(info, ascent, descent, lineGap);
}

int __stdcall StbGetFontVMetricsOS2(const stbtt_fontinfo* info, int* typoAscent, int* typoDescent, int* typoLineGap, int* winAscent, int* winDescent)
{
	return stbtt_GetFontVMetricsOS2(info, typoAscent, typoDescent, typoLineGap, winAscent, winDescent);
}

void __stdcall StbGetFontBoundingBox(const stbtt_fontinfo* info, int* x0, int* y0, int* x1, int* y1)
{
	stbtt_GetFontBoundingBox(info, x0, y0, x1, y1);
}

int __stdcall StbFindGlyphIndex(const stbtt_fontinfo* info, int codepoint)
{
	return stbtt_FindGlyphIndex(info, codepoint);
}

void __stdcall StbGetGlyphBitmapBox(const stbtt_fontinfo* info, int glyph, float scale, int* x0, int* y0, int* x1, int* y1)
{
	stbtt_GetGlyphBitmapBox(info, glyph, scale, scale, x0, y0, x1, y1);
}

void __stdcall StbMakeGlyphBitmap(const stbtt_fontinfo* info, unsigned char* output, int width, int height, int stride, int glyph, float scale)
{
	stbtt_MakeGlyphBitmap(info, output, width, height, stride, scale, scale, glyph);
}

void __stdcall StbMakeGlyphBitmapSubpixel(const stbtt_fontinfo* info, unsigned char* output, int width, int height, int stride, int glyph, float scale, float subx, float suby)
{
	stbtt_MakeGlyphBitmapSubpixel(info, output, width, height, stride, scale, scale, subx, suby, glyph);
}

void* __stdcall StbInitPackRect(int width, int height, int numNodes)
{
	stbrp_context* context = (stbrp_context*)malloc(sizeof(stbrp_context));
	stbrp_node* nodes = (stbrp_node*)malloc(sizeof(stbrp_node) * numNodes);
	stbrp_init_target(context, width, height, nodes, numNodes);
	stbrp_setup_allow_out_of_mem(context, 1);
	return context;
}

void __stdcall StbFreePackRect(stbrp_context* pack)
{
	free(pack->orig_nodes);
	free(pack);
}

int __stdcall StbPackRects(stbrp_context* pack, const int* widths, const int* heights, int* x, int* y, int num)
{
	// FONTTODO: Im on a plane, no documentation. Figure out which header has alloca.
#if defined(_MSC_VER)
	stbrp_rect* rects = (stbrp_rect*)_alloca(num * sizeof(stbrp_rect));
#elif defined(__clang__)
	stbrp_rect* rects = (stbrp_rect*)__builtin_alloca(num * sizeof(stbrp_rect));
#else
	stbrp_rect* rects = (stbrp_rect*)alloca(num * sizeof(stbrp_rect));
#endif

	for (int i = 0; i < num; i++)
	{
		rects[i].w = widths[i];
		rects[i].h = heights[i];
	}

	int result = stbrp_pack_rects(pack, rects, num);

	for (int i = 0; i < num; i++)
	{
		if (rects[i].was_packed)
		{
			x[i] = rects[i].x;
			y[i] = rects[i].y;
		}
		else
		{
			x[i] = -1;
			y[i] = -1;
		}
	}

	return result;
}

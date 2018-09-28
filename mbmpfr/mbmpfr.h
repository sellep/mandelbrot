#ifndef __MB_MPFR_H
#define __MB_MPFR_H 1

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <mpfr.h>

#define BASE 10
#define PRECISION_BITS 1024*2
#define THRESHOLD 8

typedef unsigned int uint;

typedef struct
{
	uint x;
	uint y;
} dim;

extern void zoom(
	char * const str,
	const uint str_length,
	char const * const r_min_str,
	char const * const i_min_str,
	char const * const r_max_str,
	char const * const i_max_str,
	char const * const r_point_str,
	char const * const i_point_str,
	const double percent);

extern void crop(
	char * const str,
	const uint str_length,
	char const * const r_min_str,
	char const * const i_min_str,
	char const * const r_max_str,
	char const * const i_max_str,
	const dim size,
	const dim new_width,
	const dim offset);

static inline void to_str(char * const str, const uint size, mpfr_t op)
{
	char *buf;
	buf = malloc(sizeof(char) * (size + 1));

	mpfr_exp_t e;
	mpfr_get_str(buf, &e, 10, size, op, MPFR_RNDN);

	memset(str, '0', size);

	if (e == 0)
	{
		if (buf[0] == '-')
		{
			memcpy(str + 3, buf + 1, size - 3);
			str[0] = '-';
			str[2] = '.';
		}
		else
		{
			memcpy(str + 2, buf, size - 2);
			str[1] = '.';
		}
	}
	else if (e > 0)
	{
		if (buf[0] == '-')
		{
			memcpy(str, buf, e + 1);
			memcpy(str + e + 2, buf + e + 1, size - e - 2);
			str[e + 1] = '.';
		}
		else
		{
			memcpy(str, buf, e);
			memcpy(str + e + 1, buf + e, size - e - 1);
			str[e] = '.';
		}
	}
	else
	{
		if (buf[0] == '-')
		{
			memcpy(str - e + 3, buf + 1, size + e - 3);
			str[0] = '-';
			str[2] = '.';
		}
		else
		{
			memcpy(str - e + 2, buf, size + e - 2);
			str[1] = '.';
		}
	}

	free(buf);
}

#endif
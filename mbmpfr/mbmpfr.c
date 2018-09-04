#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <mpfr.h>

#define BASE 10
#define PRECISION_BITS 1024
#define THRESHOLD 4

typedef unsigned int uint;

typedef struct
{
	uint x;
	uint y;
} dim;

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

void zoom(
	char * const str,
	const uint str_length,
	char const * const r_min_str,
	char const * const i_min_str,
	char const * const r_max_str,
	char const * const i_max_str,
	const dim size,
	const dim zoom_size,
	const dim offset)
{
	mpfr_t r_min, i_min, r_max, i_max, new_r_min, new_i_min, new_r_max, new_i_max, r_dim, i_dim;

	mpfr_init2(r_min, PRECISION_BITS);
	mpfr_init2(i_min, PRECISION_BITS);
	mpfr_init2(r_max, PRECISION_BITS);
	mpfr_init2(i_max, PRECISION_BITS);
	mpfr_init2(new_r_min, PRECISION_BITS);
	mpfr_init2(new_i_min, PRECISION_BITS);
	mpfr_init2(new_r_max, PRECISION_BITS);
	mpfr_init2(new_i_max, PRECISION_BITS);
	mpfr_init2(r_dim, PRECISION_BITS);
	mpfr_init2(i_dim, PRECISION_BITS);

	mpfr_set_str(r_min, r_min_str, BASE, MPFR_RNDN);
	mpfr_set_str(i_min, i_min_str, BASE, MPFR_RNDN);
	mpfr_set_str(r_max, r_max_str, BASE, MPFR_RNDN);
	mpfr_set_str(i_max, i_max_str, BASE, MPFR_RNDN);

	// compute ratios
	double r_min_ratio = (double)offset.x / size.x;
	double i_min_ratio = (double)offset.y / size.y;
	double width_ratio = (double)zoom_size.x / size.x;
	double height_ratio = (double)zoom_size.y / size.y;

	// compute complex dimension
	mpfr_sub(r_dim, r_max, r_min, MPFR_RNDN);
	mpfr_sub(i_dim, i_max, i_min, MPFR_RNDN);

	// compute new min
	mpfr_mul_d(new_r_min, r_dim, r_min_ratio, MPFR_RNDN);
	mpfr_mul_d(new_i_min, i_dim, i_min_ratio, MPFR_RNDN);

	mpfr_add(new_r_min, r_min, new_r_min, MPFR_RNDN);
	mpfr_add(new_i_min, i_min, new_i_min, MPFR_RNDN);

	// compute new max
	mpfr_mul_d(new_r_max, r_dim, width_ratio, MPFR_RNDN);
	mpfr_mul_d(new_i_max, i_dim, height_ratio, MPFR_RNDN);

	mpfr_add(new_r_max, r_min, new_r_max, MPFR_RNDN);
	mpfr_add(new_i_max, i_min, new_i_max, MPFR_RNDN);

	to_str(str + 0 * str_length, str_length, new_r_min);
	to_str(str + 1 * str_length, str_length, new_i_min);
	to_str(str + 2 * str_length, str_length, new_r_max);
	to_str(str + 3 * str_length, str_length, new_i_max);

	mpfr_clear(r_min);
	mpfr_clear(i_min);
	mpfr_clear(r_max);
	mpfr_clear(i_max);
	mpfr_clear(new_r_min);
	mpfr_clear(new_i_min);
	mpfr_clear(new_r_max);
	mpfr_clear(new_i_max);
	mpfr_clear(r_dim);
	mpfr_clear(i_dim);

	mpfr_free_cache();
}

void compute(
	int * const iframe,
	char const * const r_min_str,
	char const * const i_min_str,
	char const * const r_max_str,
	char const * const i_max_str,
	const dim size,
	const dim partial_size,
	const dim offset,
	const uint limit)
{
	uint x, y, i;

	mpfr_t r_min, i_min, r_max, i_max, r_step, i_step, tmp, c_r, c_i, z_r, z_i, z_r2, z_i2;

	mpfr_init2(r_min, PRECISION_BITS);
	mpfr_init2(i_min, PRECISION_BITS);
	mpfr_init2(r_max, PRECISION_BITS);
	mpfr_init2(i_max, PRECISION_BITS);
	mpfr_init2(r_step, PRECISION_BITS);
	mpfr_init2(i_step, PRECISION_BITS);
	mpfr_init2(tmp, PRECISION_BITS);
	mpfr_init2(c_r, PRECISION_BITS);
	mpfr_init2(c_i, PRECISION_BITS);
	mpfr_init2(z_r, PRECISION_BITS);
	mpfr_init2(z_i, PRECISION_BITS);
	mpfr_init2(z_r2, PRECISION_BITS);
	mpfr_init2(z_i2, PRECISION_BITS);
	
	mpfr_set_str(r_min, r_min_str, BASE, MPFR_RNDN);
	mpfr_set_str(i_min, i_min_str, BASE, MPFR_RNDN);
	mpfr_set_str(r_max, r_max_str, BASE, MPFR_RNDN);
	mpfr_set_str(i_max, i_max_str, BASE, MPFR_RNDN);

	// compute step
	mpfr_sub(r_step, r_max, r_min, MPFR_RNDN);
	mpfr_div_ui(r_step, r_step, size.x, MPFR_RNDN);
	mpfr_sub(i_step, i_max, i_min, MPFR_RNDN);
	mpfr_div_ui(i_step, i_step, size.y, MPFR_RNDN);

	// compute new min
	mpfr_mul_ui(tmp, r_step, offset.x, MPFR_RNDN);
	mpfr_add(r_min, r_min, tmp, MPFR_RNDN);
	mpfr_mul_ui(tmp, i_step, offset.y, MPFR_RNDN);
	mpfr_add(i_min, i_min, tmp, MPFR_RNDN);

	// pre cleanup
	mpfr_clear(r_max);
	mpfr_clear(i_max);

	// compute partial frame
	for (y = 0; y < partial_size.y; y++)
	{
		for (x = 0; x < partial_size.x; x++)
		{
			i = 0;

			// init c.r
			mpfr_mul_ui(tmp, r_step, x, MPFR_RNDN);
			mpfr_add(c_r, r_min, tmp, MPFR_RNDN);
			
			// init c.i
			mpfr_mul_ui(tmp, i_step, y, MPFR_RNDN);
			mpfr_add(c_i, i_min, tmp, MPFR_RNDN);
			
			// init z
			mpfr_set_ui(z_r, 0, MPFR_RNDN);
			mpfr_set_ui(z_i, 0, MPFR_RNDN);

			while (1)
			{
				// compute z squared
				//   compute z_r squared
				//   z_r^2 = z_r^2 - z_i^2
				mpfr_mul(z_r2, z_r, z_r, MPFR_RNDN);
				mpfr_mul(tmp, z_i, z_i, MPFR_RNDN);
				mpfr_sub(z_r2, z_r2, tmp, MPFR_RNDN);
			
				//   compute z_i squared
				//   z_i^2 = 2 * z_r * z_i
				mpfr_mul(z_i2, z_r, z_i, MPFR_RNDN);
				mpfr_mul_ui(z_i2, z_i2, 2, MPFR_RNDN);
				
				// compute absolute value of z squared
				// |z^2| = z_r^2 + z_i^2
				mpfr_add(tmp, z_r2, z_i2, MPFR_RNDN);
				
				// compare absolute value of z squared with threshold
				if (mpfr_cmp_ui(tmp, THRESHOLD) >= 0)
				{
					iframe[y * partial_size.x + x] = i;
					break;
				}
				
				if (++i >= limit)
				{
					iframe[y * partial_size.x + x] = i;
					break;
				}
				
				// assign z = z^2 + c
				mpfr_add(z_r, z_r2, c_r, MPFR_RNDN);
				mpfr_add(z_i, z_i2, c_i, MPFR_RNDN);
			}
		}
	}
	
	mpfr_clear(r_min);
	mpfr_clear(i_min);
	mpfr_clear(r_step);
	mpfr_clear(i_step);
	mpfr_clear(tmp);
	mpfr_clear(c_r);
	mpfr_clear(c_i);
	mpfr_clear(z_r);
	mpfr_clear(z_i);
	mpfr_clear(z_r2);
	mpfr_clear(z_i2);
	
	mpfr_free_cache();
}
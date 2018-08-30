#include <stdio.h>
#include <stdlib.h>
#include <mpfr.h>

#define BASE 10
#define PRECISION_BITS 1024
#define THRESHOLD 4

typedef unsigned int uint;

void compute(
	int * const iframe,
	char const * const r_min_str,
	char const * const i_min_str,
	char const * const r_step_str,
	char const * const i_step_str,
	const uint width,
	const uint height,
	const uint limit)
{
	uint x, y, i, offset;
	
	mpfr_t r_min, i_min, r_step, i_step, tmp, c_r, c_i, z_r, z_i, z_r2, z_i2;

	mpfr_init2(r_min, PRECISION_BITS);
	mpfr_init2(i_min, PRECISION_BITS);
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
	mpfr_set_str(r_step, r_step_str, BASE, MPFR_RNDN);
	mpfr_set_str(i_step, i_step_str, BASE, MPFR_RNDN);
	
	for (y = 0; y < height; y++)
	{
		offset = y * width;
		
		for (x = 0; x < width; x++)
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
					iframe[offset + x] = i;
					break;
				}
				
				if (++i >= limit)
				{
					iframe[offset + x] = i;
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
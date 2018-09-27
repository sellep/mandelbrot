#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <mpfr.h>
#include "mbmpfr.h"

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
	mpfr_inits2(PRECISION_BITS, r_min, i_min, r_max, i_max, r_step, i_step, tmp, c_r, c_i, z_r, z_i, z_r2, z_i2, (mpfr_ptr) 0);
	
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
	
	mpfr_clears(r_min, i_min, r_step, i_step, tmp, c_r, c_i, z_r, z_i, z_r2, z_i2, (mpfr_ptr) 0);
	
	mpfr_free_cache();
}
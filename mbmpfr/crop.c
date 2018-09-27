#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <mpfr.h>
#include "mbmpfr.h"

void crop(
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

	mpfr_inits2(PRECISION_BITS, r_min, i_min, r_max, i_max, new_r_min, new_i_min, new_r_max, new_i_max, r_dim, i_dim, (mpfr_ptr) 0);

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

	mpfr_add(new_r_max, new_r_min, new_r_max, MPFR_RNDN);
	mpfr_add(new_i_max, new_i_min, new_i_max, MPFR_RNDN);

	to_str(str + 0 * str_length, str_length, new_r_min);
	to_str(str + 1 * str_length, str_length, new_i_min);
	to_str(str + 2 * str_length, str_length, new_r_max);
	to_str(str + 3 * str_length, str_length, new_i_max);

	mpfr_clears(r_min, i_min, r_max, i_max, new_r_min, new_i_min, new_r_max, new_i_max, r_dim, i_dim, (mpfr_ptr) 0);

	mpfr_free_cache();
}
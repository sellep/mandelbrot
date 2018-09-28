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
	mpfr_t r_min, i_min, r_max, i_max, new_r_min, new_i_min, new_r_max, new_i_max, r_dim, i_dim,
		x_off, y_off, width, height, ratio, new_width, new_height;

	mpfr_inits2(PRECISION_BITS, r_min, i_min, r_max, i_max, new_r_min, new_i_min, new_r_max, new_i_max, r_dim, i_dim, 
		x_off, y_off, width, height, ratio, new_width, new_height, (mpfr_ptr) 0);

	mpfr_set_str(r_min, r_min_str, BASE, MPFR_RNDN);
	mpfr_set_str(i_min, i_min_str, BASE, MPFR_RNDN);
	mpfr_set_str(r_max, r_max_str, BASE, MPFR_RNDN);
	mpfr_set_str(i_max, i_max_str, BASE, MPFR_RNDN);

	mpfr_set_ui(x_off, offset.x, MPFR_RNDN);
	mpfr_set_ui(y_off, offset.y, MPFR_RNDN);
	mpfr_set_ui(width, size.x, MPFR_RNDN);
	mpfr_set_ui(height, size.y, MPFR_RNDN);
	mpfr_set_ui(new_width, zoom_size.x, MPFR_RNDN);
	
	// compute new_height
	mpfr_div(ratio, height, width, MPFR_RNDN);
	mpfr_mul(new_height, ratio, new_width, MPFR_RNDN);
	
	// compute complex dimension
	mpfr_sub(r_dim, r_max, r_min, MPFR_RNDN);
	mpfr_sub(i_dim, i_max, i_min, MPFR_RNDN);

	// compute new min
	mpfr_div(ratio, x_off, width, MPFR_RNDN);
	mpfr_mul(new_r_min, r_dim, ratio, MPFR_RNDN);
	mpfr_add(new_r_min, r_min, new_r_min, MPFR_RNDN);
	
	mpfr_div(ratio, y_off, height, MPFR_RNDN);
	mpfr_mul(new_i_min, i_dim, ratio, MPFR_RNDN);
	mpfr_add(new_i_min, i_min, new_i_min, MPFR_RNDN);

	// compute new max
	mpfr_div(ratio, new_width, width, MPFR_RNDN);
	mpfr_mul(new_r_max, r_dim, ratio, MPFR_RNDN);
	
	mpfr_div(ratio, new_height, height, MPFR_RNDN);
	mpfr_mul(new_i_max, i_dim, ratio, MPFR_RNDN);

	mpfr_add(new_r_max, new_r_min, new_r_max, MPFR_RNDN);
	mpfr_add(new_i_max, new_i_min, new_i_max, MPFR_RNDN);

	to_str(str + 0 * str_length, str_length, new_r_min);
	to_str(str + 1 * str_length, str_length, new_i_min);
	to_str(str + 2 * str_length, str_length, new_r_max);
	to_str(str + 3 * str_length, str_length, new_i_max);

	mpfr_clears(r_min, i_min, r_max, i_max, new_r_min, new_i_min, new_r_max, new_i_max, r_dim, i_dim,
		x_off, y_off, width, height, ratio, new_width, new_height, (mpfr_ptr) 0);
	mpfr_free_cache();
}
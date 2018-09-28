#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <mpfr.h>
#include "mbmpfr.h"

#define STR_LEN 128

static void test_zoom();

static void print_per_off(
	char const * const r_min_str,
	char const * const i_min_str,
	char const * const r_max_str,
	char const * const i_max_str,
	char const * const r_point_str,
	char const * const i_point_str);
	


int main()
{

}

static void test_zoom()
{
	char r_min_str[STR_LEN + 1] = "-2.5";
	char i_min_str[STR_LEN + 1] = "-1.5";
	char r_max_str[STR_LEN + 1] = "1";
	char i_max_str[STR_LEN + 1] = "1.5";
	
	char *r_point_str = "-0.48316056822541296";
	char *i_point_str = "-0.62553681606118200";
	int i;
	
	char str[4 * STR_LEN];	
	
	for (i = 0; i < 2000; i++)
	{
		zoom(str, STR_LEN, r_min_str, i_min_str, r_max_str, i_max_str, r_point_str, i_point_str, 0.01);
		
		memcpy(r_min_str, str + STR_LEN * 0, STR_LEN);
		memcpy(i_min_str, str + STR_LEN * 1, STR_LEN);
		memcpy(r_max_str, str + STR_LEN * 2, STR_LEN);
		memcpy(i_max_str, str + STR_LEN * 3, STR_LEN);
		
		r_min_str[STR_LEN] = '\0';
		i_min_str[STR_LEN] = '\0';
		r_max_str[STR_LEN] = '\0';
		i_max_str[STR_LEN] = '\0';		
	}
	
	print_per_off(r_min_str, i_min_str, r_max_str, i_max_str, r_point_str, i_point_str);
}


static void print_per_off(
	char const * const r_min_str,
	char const * const i_min_str,
	char const * const r_max_str,
	char const * const i_max_str,
	char const * const r_point_str,
	char const * const i_point_str)
{
	mpfr_t r_min, i_min, r_max, i_max, r_dim, i_dim, r_point, i_point, r_off, i_off, r_per, i_per, r_tmp, i_tmp;
	mpfr_inits2(PRECISION_BITS, r_min, i_min, r_max, i_max, r_dim, i_dim, r_point, i_point, r_off, i_off, r_per, i_per, r_tmp, i_tmp, (mpfr_ptr)0);
	
	mpfr_set_str(r_min, r_min_str, BASE, MPFR_RNDN);
	mpfr_set_str(i_min, i_min_str, BASE, MPFR_RNDN);
	mpfr_set_str(r_max, r_max_str, BASE, MPFR_RNDN);
	mpfr_set_str(i_max, i_max_str, BASE, MPFR_RNDN);
	mpfr_set_str(r_point, r_point_str, BASE, MPFR_RNDN);
	mpfr_set_str(i_point, i_point_str, BASE, MPFR_RNDN);
	
	// dim
	mpfr_sub(r_dim, r_max, r_min, MPFR_RNDN);
	mpfr_sub(i_dim, i_max, i_min, MPFR_RNDN);
	
	// off
	mpfr_sub(r_off, r_point, r_min, MPFR_RNDN);
	mpfr_sub(i_off, i_point, i_min, MPFR_RNDN);
	
	// 1 / dim * off
	mpfr_ui_div(r_tmp, 1, r_dim, MPFR_RNDN);
	mpfr_ui_div(i_tmp, 1, i_dim, MPFR_RNDN);
	mpfr_mul(r_per, r_tmp, r_off, MPFR_RNDN);
	mpfr_mul(i_per, i_tmp, i_off, MPFR_RNDN);

	char r[STR_LEN];
	char i[STR_LEN];
	to_str(r, STR_LEN, r_per);
	to_str(i, STR_LEN, i_per);
	
	mpfr_clears(r_min, i_min, r_max, i_max, r_dim, i_dim, r_point, i_point, r_off, i_off, r_per, i_per, r_tmp, i_tmp, (mpfr_ptr)0);
	mpfr_free_cache();
	
	printf("%.*s - %.*s\n", STR_LEN, r, STR_LEN, i);	
}
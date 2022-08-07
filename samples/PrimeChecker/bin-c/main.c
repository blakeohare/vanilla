#include <stdio.h>
#include "gen/gen.h"

int main() {
	printf("Hello, World!\n");
	VContext* vctx = create_context();
	Value* result = fn_findPrimes(vctx, vutil_get_int(vctx, 20));
	Value* numList = vutil_get_map_string_value(result, vutil_get_string_from_chars(vctx, "nums"));
	int size = 0;
	int* nums = vutil_list_to_int_array(numList, &size);
	for (int i = 0; i < size; i++) {
		if (i > 0) printf(", ");
		printf("%d", nums[i]);
	}
	printf("\n");

	return 0;
}

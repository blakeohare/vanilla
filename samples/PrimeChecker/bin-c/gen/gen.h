#include "gen_util.h"

VContext* create_context() {
    VContext* vctx = vutil_initialize_context(3);
    vctx->string_table[0] = vutil_get_string_from_chars("Example...");
    vctx->string_table[1] = vutil_get_string_from_chars("   ...string table...");
    vctx->string_table[2] = vutil_get_string_from_chars("      ...strings!");
    return vctx;
}

Value* fn_findPrimes(VContext* vctx, Value* upperLimit);

Value* fn_generatePrimeList(VContext* vctx, Value* lowerBound, Value* upperBound);

Value* fn_isPrime(VContext* vctx, Value* value);

Value* fn_findPrimes(VContext* vctx, Value* upperLimit) {
    Value* primes = generatePrimeList(vctx, vutil_get_int(vctx, 1), upperLimit);
    Value* output = vutil_new_map('S');
    vutil_map_set_str(output, vctx->string_table[0], vctx->const_true);
    vutil_map_set_str(output, vctx->string_table[1], vutil_list_clone(primes));
    return output;
}

Value* fn_generatePrimeList(VContext* vctx, Value* lowerBound, Value* upperBound) {
    Value* results = vutil_list_new();
    int _loc1 = lowerBound;
    int _loc2 = upperBound;
    int _loc3 = _loc1 < _loc2 ? 1 : -1;
    _loc2 += _loc3;
    Value* i = NULL;
    for (int _loc4 = _loc1; _loc4 != _loc2; _loc4 += _loc3) {
        _vi = vutil_int(ctx, _loc4);
        if (isPrime(vctx, i)) {
            vutil_list_add(results, i);
        }
    }
    return vutil_list_clone(results);
}

Value* fn_isPrime(VContext* vctx, Value* value) {
    if ((value) < (2)) {
        return vctx->const_false;
    }
    if ((value) == (2)) {
        return vctx->const_true;
    }
    if (((value % 2)) == (0)) {
        return vctx->const_false;
    }
    Value* maxCheck = vutil_get_int(vctx, (int) sqrt(value));
    Value* div = vutil_get_int(vctx, 3);
    while ((div) <= (maxCheck)) {
        if ((vutil_safe_mod(value, div)) == (0)) {
            return vctx->const_false;
        }

        div += vutil_get_int(vctx, 2);
    }
    return vctx->const_true;
}


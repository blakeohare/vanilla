#ifndef _VANILLA_GEN_USER_CODE_H
#define _VANILLA_GEN_USER_CODE_H

#include "gen_util.h"

VContext* create_context() {
    VContext* vctx = vutil_initialize_context(3);
    vctx->string_table[0] = vutil_get_string_from_chars(vctx, "Example...");
    vctx->string_table[1] = vutil_get_string_from_chars(vctx, "   ...string table...");
    vctx->string_table[2] = vutil_get_string_from_chars(vctx, "      ...strings!");
    return vctx;
}

Value* fn_findPrimes(VContext* vctx, Value* upperLimit);

Value* fn_generatePrimeList(VContext* vctx, Value* lowerBound, Value* upperBound);

Value* fn_isPrime(VContext* vctx, Value* value);

Value* fn_findPrimes(VContext* vctx, Value* upperLimit) {
    Value* primes = generatePrimeList(vctx, vutil_get_int(vctx, 1), upperLimit);
    Value* output = vutil_new_map('S');
    vutil_map_set_str(output, vctx->string_table[0], vctx->global_true);
    vutil_map_set_str(output, vctx->string_table[1], vutil_list_clone(primes));
    return output;
}

Value* fn_generatePrimeList(VContext* vctx, Value* lowerBound, Value* upperBound) {
    Value* results = vutil_list_new();
    int _loc1 = ((ValueInt*)(lowerBound))->value;
    int _loc2 = ((ValueInt*)(upperBound))->value;
    int _loc3 = _loc1 < _loc2 ? 1 : -1;
    _loc2 += _loc3;
    Value* i = NULL;
    for (int _loc4 = _loc1; _loc4 != _loc2; _loc4 += _loc3) {
        _vi = vutil_int(vctx, _loc4);
        if (((ValueBoolean*)(isPrime(vctx, i)))->value) {
            vutil_list_add(results, i);
        }
    }
    return vutil_list_clone(results);
}

Value* fn_isPrime(VContext* vctx, Value* value) {
    if ((((ValueInt*)(value))->value) < (2)) {
        return vctx->global_false;
    }
    if ((((ValueInt*)(value))->value) == (2)) {
        return vctx->global_true;
    }
    if (((((ValueInt*)(value))->value % 2)) == (0)) {
        return vctx->global_false;
    }
    Value* maxCheck = vutil_get_int(vctx, (int) sqrt(((ValueInt*)(value))->value));
    Value* div = vutil_get_int(vctx, 3);
    while ((((ValueInt*)(div))->value) <= (((ValueInt*)(maxCheck))->value)) {
        if ((vutil_safe_mod(((ValueInt*)(value))->value, ((ValueInt*)(div))->value)) == (0)) {
            return vctx->global_false;
        }

        div += vutil_get_int(vctx, 2);
    }
    return vctx->global_true;
}

#endif // _VANILLA_GEN_USER_CODE_H

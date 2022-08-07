#ifndef _VANILLA_GEN_USER_CODE_H
#define _VANILLA_GEN_USER_CODE_H

#include <stdio.h>
#include <stdlib.h>
#include "gen_util.h"

VContext* create_context() {
    VContext* vctx = vutil_initialize_context(2);
    vctx->string_table[0] = vutil_get_string_from_chars(vctx, "ok");
    vctx->string_table[1] = vutil_get_string_from_chars(vctx, "nums");
    return vctx;
}

Value* fn_findPrimes(VContext* vctx, Value* upperLimit);

Value* fn_generatePrimeList(VContext* vctx, Value* lowerBound, Value* upperBound);

Value* fn_isPrime(VContext* vctx, Value* value);

Value* fn_findPrimes(VContext* vctx, Value* upperLimit) {
    Value* primes = fn_generatePrimeList(vctx, vutil_get_int(vctx, 1), upperLimit);
    Value* output = vutil_new_map(vctx, 1);
    vutil_map_set_str(output, vctx->string_table[0], vctx->global_true);
    vutil_map_set_str(output, vctx->string_table[1], vutil_list_clone(vctx, primes));
    return output;
}

Value* fn_generatePrimeList(VContext* vctx, Value* lowerBound, Value* upperBound) {
    Value* results = vutil_list_new(vctx);
    int _loc1 = ((ValueInt*)(lowerBound))->value;
    int _loc2 = ((ValueInt*)(upperBound))->value;
    int _loc3 = _loc1 < _loc2 ? 1 : -1;
    _loc2 += _loc3;
    Value* i = NULL;
    for (int _loc4 = _loc1; _loc4 != _loc2; _loc4 += _loc3) {
        i = vutil_get_int(vctx, _loc4);
        if (((ValueBoolean*)(fn_isPrime(vctx, i)))->value) {
            vutil_list_add(vctx, results, i);
        }
    }
    return vutil_list_clone(vctx, results);
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
    Value* maxCheck = vutil_get_int(vctx, (int) vutil_sqrt(((ValueInt*)(value))->value));
    Value* div = vutil_get_int(vctx, 3);
    while ((((ValueInt*)(div))->value) <= (((ValueInt*)(maxCheck))->value)) {
        if ((vutil_safe_mod(((ValueInt*)(value))->value, ((ValueInt*)(div))->value)) == (0)) {
            return vctx->global_false;
        }

        div = vutil_get_int(vctx, ((ValueInt*)(div))->value+2);
    }
    return vctx->global_true;
}

#endif // _VANILLA_GEN_USER_CODE_H

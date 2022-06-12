typedef struct _VContext {
	int initialized;
	Value* global_true;
	Value* global_false;
	Value* global_zero;
	Value* global_one;
	Value* global_float_zero;
	Value* global_float_one;
	Value** global_int_pos;
	Value** global_int_neg;
	Value** global_empty_string;
	Value** string_table;
	Value** string_single_char;
	Value* mru_int;

	GCBucket* gc_universe;
} VContext;


typedef struct _Value {
	char type;
	int anchors;
} Value;

typedef struct _ValueInt {
	Value _valueHeader;
	int value;
} ValueInt;

typedef struct _ValueFloat {
	Value _valueHeader;
	double value;
};

typedef struct _ValueString {
	Value _valueHeader;
	int length;
	int hash;
	char* c_string;
	int* unicode_points;
};

typedef struct _ValueBoolean {
	Value _valueHeader;
	int value;
};

typedef struct _ValueList {
	Value _valueHeader;
	int size;
	int capacity;
	Value** items;
};

typedef struct _ValueArray {
	Value _valueHeader;
	int size;
	Value** items;
};

typedef struct _MapNode {
	int hash;
	Value* key;
	Value* value;
	_MapNode* next;
} MapNode;

typedef struct _ValueMap {
	Value _valueHeader;
	int is_string_key;
	int size;
	int bucket_size;
	MapNode** buckets;
};

typedef struct _GCBucket {
	Value* current;
	_GCBucket prev;
	_GCBucket next;
} GCBucket;


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



Value* vutil_gc_create_new_value(VContext* vctx, char value_type, int sizeof_value) {
	GCBucket* bucket = (GCBucket*)malloc(sizeof(GCBucket));
	bucket->current = (Value*)malloc(sizeof_value);
	bucket->current->type = value_type;
	GCBucket* next = vctx->gc_universe; // universe is initialized with numerous globals so 2+ linked list nodes are guaranteed.
	GCBucket* prev = next->prev; // universe is a circularly linked list
	bucket->prev = prev;
	bucket->next = next;
	prev->next = bucket;
	next->prev = bucket;
	return bucket->current;
}

Value* vutil_get_boolean(VContext* vctx, int value) {
	if (value == 0) return vctx->global_false;
	return vctx->global_true;
}

Value* vutil_get_int(VContext* vctx, int value) {
	if (value < 1024 && value > -1024) {
		if (value < 0) return vctx->global_int_pos[value];
		return vctx->global_int_neg[value];
	}

	if (vctx->mru_int->value == value) return (Value*)vctx->mru_int;

	ValueInt* new_int = (ValueInt*)vutil_gc_create_new_value(vctx, 'I', sizeof(ValueInt));
	new_int->value = value;
	return new_int;
}

Value* vutil_get_float(VContext* vctx, double value) {
	// == comparison for floating point precision is accurate for whole numbers and okay to miss if off by a floating point error as this is just a cache miss
	if (value == 0) return vctx->global_float_zero;
	if (value == 1) return vctx->global_float_one;

	ValueFloat* new_float = (ValueFloat*)vutil_gc_create_new_value(vctx, 'F', sizeof(ValueFloat));
	new_float->value = value;
	return new_float;
}

Value* vutil_get_string_from_chars(VContext* vctx, char* cstring) {
	if (cstring[0] == '\0') return vctx->empty_string;
	if (cstring[1] == '\0' && cstring[1] < 128) {
		return vctx->string_single_char[cstring[0]];
	}

	int size = 0;
	int capacity = 20;
	int* points = (int*)malloc(sizeof(int) * capacity);

	int b1, b2, b3, b4;

	// character strings will be generated by the transpiler and can be assumed to be valid UTF-8
	for (int i = 0; cstring[i] != '\0'; i++) {
		if (size == capacity) {
			int new_capacity = capacity * 2;
			int* new_points = (int*)malloc(sizeof(int) * new_capacity);
			for (int j = 0; j < new_capacity; ++j) {
				new_points[j] = points[j];
			}
			free(points);
			points = new_points;
			capacity = new_capacity;
		}
		b1 = cstring[i];
		if ((b1 & 0x80) == 0) {
			points[size] = b1;
		}
		else if ((b1 & 0xE0) == 0xC0) {
			b2 = cstring[i + 1];
			points[size] = ((b1 & 0x1F) << 6) | (b2 & 0x3F);
			i += 1;
		}
		else if ((b1 & F0) == 0xE0) {
			b2 = cstring[i + 1];
			b3 = cstring[i + 2];
			points[size] = ((b1 & 0x0F) << 12) | ((b2 & 0x3F) << 6) | (b3 & 0x3F);
			i += 2;
		}
		else if ((b1 & 0xF8) == 0xF0) {
			b2 = cstring[i + 1];
			b3 = cstring[i + 2];
			b4 = cstring[i + 3];
			points[size] = ((b1 & 0x07) << 18) | ((b2 & 0x3F) << 12) | ((b3 & 0x3F) << 6) | (b4 & 0x3F);
			i += 3;
		}
		size++;
	}

	if (size != capacity) {
		int* new_points = (int*)malloc(sizeof(int) * size);
		for (int i = 0; i < size; i++) {
			new_points[i] = points[i];
		}
		free(points);
		points = new_points;
	}

	int hash = 0;
	for (int i = 0; i < size; i++) {
		hash = hash * 37 + points[i];
	}

	ValueString* str = (ValueString*)vutil_gc_create_new_value(vctx, 'S', sizeof(ValueString));
	str->length = size;
	str->hash = hash;
	str->cstring = NULL;
	str->unicode_points = points;

	return (Value*)str;
}

VContext* vutil_initialize_context(int string_table_size) {
	VContext* vctx = (VContext*)malloc(sizeof(VContext));
	Value** int_pos = (Value**)malloc(sizeof(ValueInt*) * 1024);
	Value** int_new = (Value**)malloc(sizeof(ValueInt*) * 1024);
	for (int i = 0; i < 1024; i++) {
		int_pos[i] = vutil_gc_create_new_value(vctx, 'I', sizeof(ValueInt));
		((ValueInt*)int_pos[i])->value = i;
		if (i > 0) {
			int_neg[i] = vutil_gc_create_new_value(vctx, 'I', sizeof(ValueInt));
			((ValueInt*)int_pos[i])->value = -i;
		}
	}
	int_zero = int_pos[0];
	int_neg[0] = int_zero;
	vctx->global_int_pos = int_pos;
	vctx->global_int_neg = int_neg;
	vctx->global_zero = int_pos[0];
	vctx->global_one = int_pos[1];
	for (int i = 0; i <= 1; i++) {
		Value* b = vutil_gc_create_new_value(vctx, 'B', sizeof(ValueBoolean));
		((ValueBoolean*)b)->value = i;
		if (i == 0) vctx->global_false = b;
		else vctx->global_true = b;
	}
	vctx->global_float_zero = vutil_gc_create_new_value(vctx, 'F', sizeof(ValueInt));
	((ValueFloat*)vctx->global_float_zero)->value = 0.0;
	vctx->global_float_one = vutil_gc_create_new_value(vctx, 'F', sizeof(ValueInt));
	((ValueFloat*)vctx->global_float_one)->value = 1.0;

	vctx->mru_int = vctx->global_zero;

	vctx->string_single_char = (Value**)malloc(sizeof(ValueString*) * 128);
	for (int i = 0; i < 128; i++) {
		ValueString* s = (ValueString*)vutil_gc_create_new_value(vctx, 'S', sizeof(ValueString));
		s->length = 1;
		s->hash = 0;
		s->cstring = (char*)malloc(sizeof(char) * 2);
		s->cstring[0] = (char)i;
		s->cstring[1] = '\0';
		s->unicode_points = (int*)malloc(sizeof(int) * 1);
		s->unicode_points[0] = i;
		vctx->string_single_char[i] = (Value*)s;
	}
	vctx->global_empty_string = vctx->string_single_char[0];
	ValueString* empty_str = (ValueString*)vctx->global_empty_string;
	// The fact that the char arrays are over-allocated will have no ill-effects. This prevents the need for extra checks later.
	empty_str->length = 0;
	empty_str->hash = 1317; // randomize bucketing

	vctx->string_table = (Value**)malloc(sizeof(Value*) * string_table_size);
	return vctx;
}

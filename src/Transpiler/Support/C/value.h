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

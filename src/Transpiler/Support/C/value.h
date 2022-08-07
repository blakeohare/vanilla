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
} ValueFloat;

typedef struct _ValueString {
	Value _valueHeader;
	int length;
	int hash;
	char* c_string;
	int* unicode_points;
} ValueString;

typedef struct _ValueBoolean {
	Value _valueHeader;
	int value;
} ValueBoolean;

typedef struct _ValueList {
	Value _valueHeader;
	int size;
	int capacity;
	Value** items;
} ValueList;

typedef struct _ValueArray {
	Value _valueHeader;
	int size;
	Value** items;
} ValueArray;

typedef struct _MapNode {
	int hash;
	Value* key;
	Value* value;
	struct _MapNode* next;
} MapNode;

typedef struct _ValueMap {
	Value _valueHeader;
	int is_string_key;
	int size;
	int bucket_size;
	MapNode** buckets;
} ValueMap;

typedef struct _GCBucket {
	Value* current;
	struct _GCBucket* prev;
	struct _GCBucket* next;
} GCBucket;

typedef struct _List {
	Value value;
	int length;
	int capacity;
	Value** items;
} List;


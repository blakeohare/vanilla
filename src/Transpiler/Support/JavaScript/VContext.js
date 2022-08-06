const createVanillaContext = () => {

	let constTrue = { type: 'B', value: true };
	let constFalse = { type: 'B', value: false };
	let numPos = [];
	let numNeg = [];
	for (let i = 0; i < 1000; i++) {
		numPos.push({ type: 'I', value: i });
		numNeg.push({ type: 'I', value: -i });
	}
	let constZero = numPos[0];
	numNeg[0] = constZero;
	let constOne = numPos[1];
	let commonStrings = {};
	let emptyString = { type: 'S', value: "" };
	commonStrings[""] = emptyString;

	return {
		constTrue,
		constFalse,
		numPos,
		numNeg,
		constZero,
		constOne,
		commonStrings,
		emptyString,
	};
};

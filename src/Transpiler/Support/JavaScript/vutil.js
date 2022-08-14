const createVutil = (vctx) => {
	let vutilGetCommonString = (s) => {
		let ws = vctx.commonStrings[s];
		if (ws === undefined) {
			ws = { type: 'S', value: s };
			vctx.commonStrings[s] = ws;
		}
		return ws;
	};
	let vutilGetInt = (n) => {
		if (n < 1000 && n > -1000) {
			if (n < 0) return vctx.numNeg[-n];
			return vctx.numPos[n];
		}
		return { type: 'I', value: n };
	};
	let vutilGetString = (s) => {
		if (s.length < 2) {
			if (s.length === 0) return vctx.emptyString;
			let o = vctx.commonStrings[s];
			if (o) return o;
			o = { type: 'S', value: s };
			vctx.commonStrings[s] = o;
			return o;
		}
		return { type: 'S', value: s };
	};
	let vutilMapSet = (wm, wk, wv) => {
		let nk = wk.value;
		let i = wm.nativeKeyToIndex[nk];
		if (i === undefined) {
			wm.nativeKeyToIndex[nk] = wm.keys.length;
			wm.keys.push(wk);
			wm.values.push(wv);
		} else {
			wm.values[i] = wv;
		}
	};
	let vutilNewInstance = (name) => {
		let obj = { '@class': name, ...vctx.classMetadata[name].methods };
		return { type: 'O', value: obj };
	};
	let vutilNewMap = (isIntKeys) => {
		return { type: 'M', keys: [], values: [], nativeKeyToIndex: {}, isIntKeys };
	};
	let vutilMapBuilder = (isIntKeys, kvpList) => {
		let m = vutilNewMap(isIntKeys);
		for (let i = 0; i < kvpList.length; i += 2) {
			vutilMapSet(m, kvpList[i], kvpList[i + 1]);
		}
		return m;
	};
	let vutilWrapMap = (m, isIntKeys) => {
		let wMap = vutilNewMap(isIntKeys);
		let wKey;
		let wValue;
		for (let k of Object.keys(m)) {
			if (isIntKeys) {
				let intKey = parseInt(k);
				wKey = vutilGetInt(intKey);
				wValue = vutilWrapNative(m[k]);
			} else {
				wKey = vutilGetString(k);
				wValue = vutilWrapNative(m[k]);
			}
			wMap.nativeKeyToIndex[k] = wMap.keys.length;
			wMap.keys.push(wKey);
			wMap.values.push(wValue);
		}
		return wMap;
	};
	let vutilSafeMod = (n, d) => {
		if (d < 0) return ((n % d) + d) % n;
		return n % d;
	};
	let vutilWrapArray = (arr) => {
		return { type: 'A', value: arr };
	};
	let vutilWrapNative = (value) => {
		if (!value) {
			if (value === null) return vctx.constNull;
			if (value === false) return vctx.constFalse;
			if (value === "") return vctx.emptyString;
			if (value === undefined) throw new Error("undefined leakage");
			throw new Error(); // should not happen.
		}
		switch (typeof value) {
			case 'number':
				if (value % 1 === 0) return vutilGetInt(value);
				if (value === 0) return vctx.constZeroF;
				return { type: 'F', value };

			case 'string':
				if (value.length === 1) {
					if (!vctx.commonStrings[value]) {
						vctx.commonStrings[value] = { type: 'S', value };
					}
					return vctx.commonStrings[value];
				}
				return { type: 'S', value };

			case 'object':
				if (Array.isArray(value)) {
					return { type: 'A', value: value.map(vutilWrapNative) };
				}
				return { type: 'M', value: vutilWrapMap(value) };

			default:
				throw new Error();
		}
	};

	// TODO: support cyclic data structures
	let vutilUnwrapNative = (value) => { // TODO: should be vutilUnwrap*To*Native
		switch (value.type) {
			case 'N':
				return null;
			case 'B':
			case 'I':
			case 'F':
			case 'S':
				return value.value;
			case 'O':
				throw new Error(); // TODO: unwrap objects
			case 'A':
				return value.value.map(vutilUnwrapNative);
			case 'M':
				{
					let m = {};
					let len = value.keys.length;
					for (let i = 0; i < len; i++) {
						m[value.keys[i].value] = vutilUnwrapNative(value.values[i]);
					}
					return m;
				}
			default:
				throw new Error(); // not implemented.
		}
	};

	return {
		vutilGetCommonString,
		vutilGetInt,
		vutilGetString,
		vutilMapBuilder,
		vutilMapSet,
		vutilNewInstance,
		vutilNewMap,
		vutilSafeMod,
		vutilUnwrapNative,
		vutilWrapArray,
		vutilWrapMap,
		vutilWrapNative,
	};
};

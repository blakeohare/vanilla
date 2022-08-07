window.addEventListener("load", () => {
	let numEntry = document.getElementById('upper-limit');
	let findBtn = document.getElementById('find-btn');
	let outputDiv = document.getElementById('output-panel');
	
	findBtn.addEventListener("click", () => {
		let upperLimit = parseInt(numEntry.value);
		if (isNaN(upperLimit) || upperLimit < 1) {
			outputDiv.innerText = "Upper limit must be a positive integer.";
		} else {
			let result = findPrimes(upperLimit);
			if (result.ok) {
				if (result.nums.length === 0) {
					outputDiv.innerText = "There are no primes";
				} else {
					outputDiv.innerText = "Primes: " + result.nums.join(', ');
				}
			}
		}
	});
});

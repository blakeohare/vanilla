@public
function:map<string, object> findPrimes(const:int upperLimit) {
    const:array<int> primes = generatePrimeList(1, upperLimit);
    const:map<string, object> output = map<string, object>.of();
    output["ok"] = true;
    output["nums"] = array<object>.from(primes);
    return output;
}

function:array<int> generatePrimeList(const:int lowerBound, const:int upperBound) {
    const:list<int> results = list<int>.of();
    for (const:int i from lowerBound thru upperBound) {
        if (isPrime(i)) {
            results.add(i);
        }
    }
return results.toArray();
}

function:bool isPrime(const:int value) {
    if (value < 2) return false;
    if (value == 2) return true;
    if (value % 2 == 0) return false;
    const:int maxCheck = $floor($sqrt(value));
    for (var:int div = 3; div <= maxCheck; div += 2) {
        if (value % div == 0) {
            return false;
        }
    }
    return true;
}

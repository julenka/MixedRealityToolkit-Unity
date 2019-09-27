def test_str(j, s, i):
    return "[UnityTest]\npublic IEnumerator A%i%s%i(){ yield return %s();}" % ( i, s, j, s)

def print_tests(testnames, repeat):
    for i in range(1, repeat):
        for j, name in enumerate(testnames):
            print(test_str(j, name,  i))

def main(argv):
    import argparse
    parser = argparse.ArgumentParser(description="Print code to paste in that repeats tests")
    parser.add_argument("-t", "--tests", nargs="+", help="list of tests to repeat")
    parser.add_argument("-n", "--count", type=int, default=100, help="number of times to repeat test sequence (default 100)")

    args = parser.parse_args(argv[1:])
    if (args):
        print_tests(args.tests, args.count)

if __name__ == "__main__":
    import sys
    main(sys.argv)

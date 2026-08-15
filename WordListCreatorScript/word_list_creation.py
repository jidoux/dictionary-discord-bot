from wordfreq import word_frequency
from json import dump

# I had initially wrote the "word of the day/word learning generator" bot in python with the word_frequency package, but
# then switched the bot's code from python to C# because I got sick of the dynamic typing and wanted to use a language
# I was better with to speed things up. However I liked this word_freq functionality so I'm keeping this python
# dependency and generating the acutal word pool in python before having the C# code actually run the bot and whatever.
# This just needs to be ran manually whenever the word pool should change.

# NOTE:
# the english.txt was downloaded from https://github.com/meetDeveloper/freeDictionaryAPI/blob/master/meta/wordList/english.txt on 7/3/2026.

# Could always add more things here if need be... but I don't want to go overboard with suffix/prefix checking.
def word_is_valid(word: str) -> bool:
    freq = word_frequency(word, 'en')
    # This value was just chosen through my testing. It seems good, to be honest, but may not be optimal, yet.
    arbitrary_frequency_value_to_use = 1e-6

    if freq > arbitrary_frequency_value_to_use:
        return False
    # The list of words I'm using sometimes contains words with spaces; I don't want words with spaces.
    if " " in word:
        return False
    if word.endswith("ing"):
        return False
    if word.endswith("ed"):
        return False
    return True

def main():
    word_list = []
    input_file_name = "english.txt"
    output_file_name = "output/word_pool.json"
    encoding = "utf-8"
    with open(input_file_name, "r", encoding=encoding) as infile:
        for line in infile:
            word = line.strip()
            if word_is_valid(word):
                word_list.append(word)
    with open(output_file_name, "w", encoding=encoding) as outfile:
        dump(word_list, outfile)

if __name__ == '__main__':
    main()

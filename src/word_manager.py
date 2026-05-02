from wordfreq import word_frequency
from requests import get
from random import choice

class WordManager:
    def __init__(self):
        self._word_list: list[str] = []
        # This file was found from the following repo on 2/16/2026:
        # https://github.com/meetDeveloper/freeDictionaryAPI/blob/master/meta/wordList/english.txt
        self.__populate_word_list("src/english.txt", "utf-8") 
    
    def get_word_and_printable_definition(self) -> tuple[str, str]:
        while True:
            word = choice(self._word_list)
            definition_response: tuple[int, str] = self.__get_definition(word)
            # bad solution to the problem of some definitions maybe not being there, which can happen even though the
            # list of words is directly from the API's repo.
            if definition_response[0] == 404:
                continue
            elif definition_response[0] != 200:
                print(f"Some disasterous error occurred, idk why. Error code: {definition_response[0]}")
                # Assuming that if the API is failing or something, I don't want to retry at the present time. I think
                # just showing a word but no definition is perfectly OK, to be honest.
                return word, f"An error occurred - could not fetch definition for some reason (error code {definition_response[0]})"
            else:
                # Removing the word from the in-memory word list to prevent resends;
                # obviously database persistence would be better, but this is fine assuming good uptime.
                self._word_list.remove(word)
                return word, definition_response[1]
        
    # Gets the definition of the word, returning the API response status code and a definition string.
    # The string can be null iff there is a bad response or exception. The status code is -1 on exception.
    def __get_definition(self, word: str) -> tuple[int, str]:
        try:
            url = f"https://api.dictionaryapi.dev/api/v2/entries/en/{word}"
            response = get(url)
            response_string = None
    
            if response.status_code == 200:
                data = response.json()
                # I just get the first definition here. Not sure if this is the best move
                meaning = data[0]['meanings'][0]
                definition = meaning['definitions'][0]['definition']
                part_of_speech = meaning['partOfSpeech']
                response_string = f"**{word}** (*{part_of_speech}*): {definition}"
            return response.status_code, response_string
        except Exception as e:
            print(f"__get_definition exception1: {str(e)}")
            return -1, None

    # Could always add more things here if need be... but I don't want to go overboard with suffix/prefix checking.
    def __word_is_valid(self, word: str) -> bool:
        freq = word_frequency(word, 'en')
        if freq > 1e-6:
            return False
        # The list of words I'm using sometimes contains words with spaces; I don't want words with spaces.
        if " " in word:
            return False
        word_ends_with_ing = len(word) > 3 and word[-3:] == "ing"
        if word_ends_with_ing:
            return False
        word_ends_with_ed = len(word) > 2 and word[-2:] == "ed"
        if word_ends_with_ed:
            return False
        return True

    def __populate_word_list(self, word_list_file_name_with_extension: str, encoding: str) -> None:
        self._word_list.clear()  
        with open(word_list_file_name_with_extension, encoding=encoding) as infile:
            for line in infile:
                if self.__word_is_valid(line.strip()):
                    self._word_list.append(line)

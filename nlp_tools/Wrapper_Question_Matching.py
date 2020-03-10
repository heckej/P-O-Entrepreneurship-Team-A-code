import sys
from fuzzywuzzy import fuzz

# Program takes two strings as input containing the questions to be compared
question_1 = sys.argv[0]
question_2 = sys.argv[1]

"""
Method getFeatures(question1, question2) returns dictionary of features given two questions
"len_q1"                            =   length of the first string
"len_q2"                            =   length of second string
"diff_len"                          =   difference in length (len_q1-len_q2)
"len_char_q1"                       =   length of the first string without the spaces
"len_char_q2"                       =   length of the second string without the spaces
"len_word_q1"                       =   word count of the first string
"len_word_q2"                       =   word count of the second string
"common_words"                      =   count of words the two strings have in common
"fuzz_Qratio"                       =   Q ratio of the strings
"fuzz_Wratio"                       =   W ratio of the string
"fuzz_partial_ratio"                =   partial ratio of the strings
"fuzz_partial_token_set_ratio"      =   partial token set ratio
"fuzz_partial_token_sort_ratio"     =   partial token sort ratio
"fuzz_token_set_ratio"              =   token set ratio
"fuzz_token_sort_ratio"             =   token sort ratio
"""


def getFeatures(question1, question2):
    outputDict = {
        # length based features
        "len_q1": len(question1),
        "len_q2": len(question2),
        "diff_len": len(question1) - len(question2),
        "len_char_q1": len(question1.replace(" ", "")),
        "len_char_q2": len(question2.replace(" ", "")),
        "common_words": len(set(question1.lower().split()).intersection(set(question2.lower().split()))),
        # distance based features
        #   (fuzzywuzzy library tutorial: https://www.datacamp.com/community/tutorials/fuzzy-string-python)
        "fuzz_Qratio": fuzz.QRatio(question1, question2),
        "fuzz_Wratio": fuzz.WRatio(question1, question2),
        "fuzz_partial_ratio": fuzz.partial_ratio(question1, question2),
        "fuzz_partial_token_set_ratio": fuzz.partial_token_set_ratio(question1, question2),
        "fuzz_partial_token_sort_ratio": fuzz.partial_token_sort_ratio(question1, question2),
        "fuzz_token_set_ratio": fuzz.token_set_ratio(question1, question2),
        "fuzz_token_sort_ratio": fuzz.token_sort_ratio(question1, question2),

    }
    return outputDict


# this variable should be written to an output file
outputFeatures = getFeatures(question_1, question_2)
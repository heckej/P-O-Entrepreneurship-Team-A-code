from profanity_check import predict_prob

cut_off = 0.8


def is_offensive(sentence):
    profane_prob = predict_prob([sentence])
    return profane_prob[0] > 0.8


print(is_offensive("Fuck you"))
print(is_offensive("Little bitch"))
print(is_offensive("You can find a coffee machine on the second floor"))
print(is_offensive("You're so dumb you can't even find a stupid coffee machine"))
print(is_offensive(""))

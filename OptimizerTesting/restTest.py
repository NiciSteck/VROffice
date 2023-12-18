from flask import Flask, render_template, request

app = Flask(__name__)

test = {"fml" : 0}

test2 = [{"fml" : 1}]


@app.get("/test")
def getTest():
    print("getTest")
    return test

@app.get("/test2")
def getTest2():
    return test2

@app.put("/test")
def putTest():
    print("putTest")
    test = {"fml" : 2}
    return test

@app.put("/test2")
def putTest2():
    if request.is_json:
        requestJson = request.get_json()
        print(requestJson)
        unityPoints = requestJson["points"]
        sum = 0
        for point in unityPoints:
            sum += point["fml"]
            print(point)
        print(sum)
        return "nice"
    return "fuck"

if __name__ == "__main__":
    app.run(port=5005,debug = True)

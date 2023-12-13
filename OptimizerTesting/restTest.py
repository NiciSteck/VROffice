from flask import Flask, render_template, request

app = Flask(__name__)

test = {"fml" : 0}

test2 = [{"fml" : 1}]


@app.get("/test")
def getTest():
    return test

@app.get("/test2")
def getTest2():
    return test2

@app.put("/test")
def putTest():
    test = {"fml" : 2}
    return test

@app.put("/test2")
def putTest2():
    unityPoints = request.get_json()["points"]
    sum = 0
    for point in unityPoints:
        sum += point["fml"]
        print(point)
    print(sum)
    return "fuck", 200

if __name__ == "__main__":
    app.run(port=5005,debug = True)

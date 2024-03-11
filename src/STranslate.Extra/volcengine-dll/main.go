package main

import (
	"C"
	"encoding/json"
	"fmt"
	"net/http"
	"net/url"
	"time"

	"github.com/volcengine/volc-sdk-golang/base"
)

func main() {
	//_, str := Execute(kAccessKey, kSecretKey, "auto", "zh", "hello world")
	//println(str)
}

//export TestReceive
func TestReceive(msg *C.char) string {
	// 输出参数
	var str string
	str = C.GoString(msg)
	return fmt.Sprintf("success to receive: %v", str)
}

//export TestRunturn
func TestRunturn() string { return "success" }

//export TestMultiReturn
func TestMultiReturn() (int, string) {
	return 0, "success"
}

//export Execute
func Execute(accessKey, secretKey, source, target, content *C.char) (int, string) {
	client := base.NewClient(ServiceInfo, ApiInfoList)
	client.SetAccessKey(C.GoString(accessKey))
	client.SetSecretKey(C.GoString(secretKey))
	var req = Req{
		TargetLanguage: C.GoString(target),
		TextList:       []string{C.GoString(content)},
	}
	if C.GoString(source) != "auto" {
		req.SourceLanguage = C.GoString(source)
	}
	body, _ := json.Marshal(req)

	resp, code, err := client.Json("TranslateText", nil, string(body))
	if err != nil {
		return 500, err.Error()
	}
	fmt.Println(string(resp))
	return code, string(resp)
}

const (
	kAccessKey      = "xx" // https://console.volcengine.com/iam/keymanage/
	kSecretKey      = "xx"
	kServiceVersion = "2020-06-01"
)

var (
	ServiceInfo = &base.ServiceInfo{
		Timeout: 10 * time.Second,
		Host:    "translate.volcengineapi.com",
		Header: http.Header{
			"Accept": []string{"application/json"},
		},
		Credentials: base.Credentials{Region: base.RegionCnNorth1, Service: "translate"},
	}
	ApiInfoList = map[string]*base.ApiInfo{
		"TranslateText": {
			Method: http.MethodPost,
			Path:   "/",
			Query: url.Values{
				"Action":  []string{"TranslateText"},
				"Version": []string{kServiceVersion},
			},
		},
	}
)

type Req struct {
	SourceLanguage string   `json:"SourceLanguage"`
	TargetLanguage string   `json:"TargetLanguage"`
	TextList       []string `json:"TextList"`
}

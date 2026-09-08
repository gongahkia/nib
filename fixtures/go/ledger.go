package ledger

import (
	"errors"
	"fmt"
	"strings"
)

type Entry struct {
	Title   string
	Tags    []string
	Settled bool
}

func Parse(raw string) (Entry, error) {
	parts := strings.Split(raw, "#")
	title := strings.TrimSpace(parts[0])
	if title == "" {
		return Entry{}, errors.New("title is required")
	}
	return Entry{Title: title, Tags: parts[1:]}, nil
}

func (entry Entry) Summary() string {
	return fmt.Sprintf("%s [%s]", entry.Title, strings.Join(entry.Tags, ", "))
}

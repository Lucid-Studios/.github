(defpackage :oan-sli.packet-parser
  (:use :cl)
  (:export :parse-packet :normalize-packet :packet-get))

(in-package :oan-sli.packet-parser)

(defparameter *required-keys* '(:env :frame :mode :op))
(defparameter *optional-keys* '(:timestamp :payload :identity))

(defun packet-get (packet key)
  (getf (rest packet) key))

(defun %assert-plist-shape (items)
  (unless (evenp (length items))
    (error "Packet fields must be key/value pairs."))
  (loop for key in items by #'cddr
        do
           (unless (keywordp key)
             (error "Packet key ~S must be a keyword." key))))

(defun normalize-packet (packet)
  (let ((items (rest packet))
        (normalized (list :packet))
        (extras '()))
    (dolist (key *required-keys*)
      (let ((value (getf items key :__missing__)))
        (unless (eq value :__missing__)
          (setf normalized (append normalized (list key value))))))
    (dolist (key *optional-keys*)
      (let ((value (getf items key :__missing__)))
        (unless (eq value :__missing__)
          (setf normalized (append normalized (list key value))))))
    (loop for (key value) on items by #'cddr
          unless (or (member key *required-keys*)
                     (member key *optional-keys*))
            do (setf extras (append extras (list key value))))
    (append normalized extras)))

(defun parse-packet (form)
  (unless (and (listp form)
               form
               (symbolp (first form))
               (string-equal (symbol-name (first form)) "PACKET"))
    (error "Input is not a canonical (packet ...) form."))
  (let ((items (rest form)))
    (%assert-plist-shape items)
    (normalize-packet (cons :packet items))))

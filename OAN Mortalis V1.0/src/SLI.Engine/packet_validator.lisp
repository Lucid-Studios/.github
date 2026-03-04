(defpackage :oan-sli.packet-validator
  (:use :cl :oan-sli.packet-parser)
  (:export :validate-packet))

(in-package :oan-sli.packet-validator)

(defparameter *required-keys* '(:env :frame :mode :op))
(defparameter *valid-modes* '("EMIT" "QUERY" "REPLY" "COMMAND"))

(defun %symbol-name-equals (value expected)
  (and (symbolp value)
       (string-equal (symbol-name value) expected)))

(defun %missing-required-keys (packet)
  (loop for key in *required-keys*
        unless (packet-get packet key)
          collect key))

(defun %mother-authority-valid-p (packet)
  (let ((env (packet-get packet :env))
        (frame (packet-get packet :frame))
        (mode (packet-get packet :mode)))
    (and (symbolp env)
         (symbolp frame)
         (symbolp mode)
         (member (symbol-name mode) *valid-modes* :test #'string-equal))))

(defun %father-authority-valid-p (packet)
  (let ((operation (packet-get packet :op)))
    (and operation
         (symbolp operation)
         (not (%symbol-name-equals operation "NIL")))))

(defun validate-packet (packet)
  (let ((missing (%missing-required-keys packet)))
    (cond
      (missing
       (list :valid nil :reason :missing-required-fields :fields missing))
      ((not (%mother-authority-valid-p packet))
       (list :valid nil :reason :mother-sli-rejected))
      ((not (%father-authority-valid-p packet))
       (list :valid nil :reason :father-sli-rejected))
      (t
       (list :valid t :reason :accepted)))))
